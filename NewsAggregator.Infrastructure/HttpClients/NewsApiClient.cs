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

        public NewsApiClient(HttpClient httpClient, NewsStorageAppService newsStorageService)
        {
            _httpClient = httpClient;
            _newsStorageService = newsStorageService;
        }

        public async Task FetchAndStoreNewsAsync(string q)
        {
            try
            {
                string url = "https://newsapi.org/v2/everything?q=" + q;

                // Create a request object
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Add API Key Header
                request.Headers.Add("X-Api-Key", "d87a2248207c4271a8bdd70cd91fb2e4");
                request.Headers.Add("User-Agent", "NewsAggregatorAPI/1.0");

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
