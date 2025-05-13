using Microsoft.AspNetCore.RateLimiting;
using News.Application.Interfaces;
using News.Application.Services;
using News.Infrastructure;
using Notification.Infrastructure.Messaging;
using Serilog;
using System.Threading.RateLimiting;

public partial class Program
{
    public static async Task<WebApplication> CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            builder.Host.UseSerilog();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            builder.Services.AddNewsInfrastructureServices(builder.Configuration);
            builder.Services.AddScoped<INewsAppService, NewsAppService>();
            builder.Services.AddScoped<NewsStorageAppService>();
            builder.Services.AddScoped<IArticleEventPublisher, ArticleEventPublisher>();

            builder.Services.AddControllers();
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "News Aggregator API", Version = "v1" });
            });

            builder.Services.AddMemoryCache();

            // Add Rate Limiting services
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Set global status code for rejected requests

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterVal)
                        ? retryAfterVal.TotalSeconds
                        : 60; // Default fallback

                    var errorResponse = new
                    {
                        error = "Too many requests. Please try again later.",
                        retryAfterSeconds = retryAfter
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
                };

                options.AddFixedWindowLimiter(policyName: "fixed", configureOptions: limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5; // Max 5 requests
                    limiterOptions.Window = TimeSpan.FromMinutes(1); // Every 1 minute
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0; // Disable queueing: reject immediately
                });
            });

            var app = builder.Build();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            // Enable Rate Limiting
            app.UseRateLimiter();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "NewsAPI terminated unexpectedly!");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static async Task Main(string[] args)
    {
        var app = await CreateApp(args);
        app.Run();
    }
}