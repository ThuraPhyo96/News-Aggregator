using Microsoft.Extensions.Logging;
using News.Application.Services;
using News.Domain.Models;
using News.Infrastructure.ExternalModels;
using System.Text.Json;

namespace News.Infrastructure.HttpClients
{
    public class NewsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly NewsStorageAppService _newsStorageService;
        private readonly ILogger<NewsApiClient> _logger;

        public NewsApiClient(IHttpClientFactory httpClientFactory, NewsStorageAppService newsStorageService, ILogger<NewsApiClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(NewsApiClient));
            _newsStorageService = newsStorageService;
            _logger = logger;
        }

        public async Task FetchAndStoreNewsAsync(string q)
        {
            try
            {
                string url = "/v2/everything?q=" + q;

                // Create a request object
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Send the request
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    JsonSerializerOptions options = new()
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    NewsResponse? newsResponse = JsonSerializer.Deserialize<NewsResponse>(json, options);

                    if (newsResponse?.Articles != null)
                    {
                        List<Article> articles = newsResponse.Articles;
                        //await _newsStorageService.StoreArticlesAsync(articles);
                        _logger.LogInformation("Successfully fetched news: {Length} bytes", json.Length);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch news. StatusCode: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching news");
            }
        }
    }
}
