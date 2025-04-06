using NewsAggregator.Application.Services;
using NewsAggregator.Domain.Models;
using NewsAggregator.Infrastructure.ExternalModels;
using System.Text.Json;

namespace NewsAggregator.Infrastructure.HttpClients
{
    public class NewsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly NewsStorageAppService _newsStorageService;

        public NewsApiClient(IHttpClientFactory httpClientFactory, NewsStorageAppService newsStorageService)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(NewsApiClient));
            _newsStorageService = newsStorageService;
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
                        await _newsStorageService.StoreArticlesAsync(articles);
                        Console.WriteLine("Articles stored in MongoDB successfully!");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch news. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching news: {ex.Message}");
            }
        }
    }
}
