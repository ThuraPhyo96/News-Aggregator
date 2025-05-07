using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NewsAggregator.Application.Services;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Infrastructure.HttpClients;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace NewsAggregator.IntegrationTests
{
    public class NewsApiClientWireMockTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly NewsApiClient _client;
        private readonly NewsStorageAppService _storageMock;
        private readonly ILogger<NewsApiClient> _logger;

        public NewsApiClientWireMockTests()
        {
            _server = WireMockServer.Start();
            var newsRepoMock = new Mock<INewsRepository>();
            _storageMock = new NewsStorageAppService(newsRepoMock.Object);
            _logger = new LoggerFactory().CreateLogger<NewsApiClient>();

            var factory = new HttpClientFactoryStub(_server.Urls[0]);

            _client = new NewsApiClient(factory, _storageMock, _logger);
        }

        [Fact]
        public async Task FetchAndStoreNewsAsync_ShouldLogSuccess_WhenNewsApiReturnsArticles()
        {
            // Arrange
            var fakeResponse = new
            {
                status = "OK",
                totalResults = 1,
                articles = new[]
                {
                    new
                    {
                        source = new { id = "cnn", name = "CNN" },
                        author = "John",
                        title = "Test Article",
                        description = "Description",
                        url = "http://test.com",
                        urlToImage = "http://img.com/image.jpg",
                        publishedAt = DateTime.UtcNow,
                        content = "Test content"
                    }
                }
            };

            _server
                .Given(Request.Create().WithPath("/v2/everything").UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(fakeResponse)));

            // Act
            await _client.FetchAndStoreNewsAsync("technology");

            // Assert
            _server.LogEntries.Should().ContainSingle();
        }

        // Helper HttpClientFactory
        private class HttpClientFactoryStub(string baseUrl) : IHttpClientFactory
        {
            private readonly string _baseUrl = baseUrl;

            public HttpClient CreateClient(string name)
            {
                return new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
            }
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
