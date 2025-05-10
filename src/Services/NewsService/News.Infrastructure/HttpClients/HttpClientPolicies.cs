using Microsoft.Extensions.Logging;
using Polly;
using System.Net;

namespace News.Infrastructure.HttpClients
{
    public static class HttpClientPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy<T>(ILogger<T> logger)
        {
            {
                // 1. Circuit Breaker (innermost - tracks ALL attempts)
                var circuitBreakerPolicy = Policy<HttpResponseMessage>
                    .Handle<Exception>()
                    .OrResult(r => !r.IsSuccessStatusCode)
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: 3,  // Must match retry count
                        durationOfBreak: TimeSpan.FromSeconds(30),
                        onBreak: (outcome, breakDelay) =>
                        {
                            logger?.LogWarning($"[BREAKER] Broken for {breakDelay.TotalSeconds}s. Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                        },
                        onReset: () => logger?.LogInformation("[BREAKER] Reset"),
                        onHalfOpen: () => logger?.LogInformation("[BREAKER] Testing half-open"));

                // 2. Retry Policy (middle layer - now reports failures to breaker)
                var retryPolicy = Policy<HttpResponseMessage>
                    .Handle<Exception>()
                    .OrResult(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3,
                        attempt => TimeSpan.FromSeconds(1),
                        (outcome, delay, retryCount, context) =>
                        {
                            logger?.LogInformation($"[RETRY] Attempt {retryCount}. Status: {outcome.Result?.StatusCode}");
                            // Ensure failures propagate to circuit breaker
                            if (retryCount == 3) throw new Exception("Retries exhausted");
                        });

                // 3. Fallback (outermost)
                var fallbackPolicy = Policy<HttpResponseMessage>
                    .Handle<Exception>()
                    .OrResult(msg => !msg.IsSuccessStatusCode)
                    .FallbackAsync(
                        fallbackAction: ct =>
                        {
                            logger?.LogInformation("Executing fallback logic.");
                            var fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent("{\"articles\":[]}")
                            };
                            return Task.FromResult(fallbackResponse);
                        });

                return Policy.WrapAsync(fallbackPolicy, retryPolicy, circuitBreakerPolicy);
            }
        }
    }
}
