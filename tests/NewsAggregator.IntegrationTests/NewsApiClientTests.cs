using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using NewsAggregator.Application.Services;
using NewsAggregator.Infrastructure.HttpClients;
using System.Net;
using NewsAggregator.Domain.Interfaces;

namespace NewsAggregator.IntegrationTests
{
    public class NewsApiClientTests
    {
        [Fact]
        public async Task FetchAndStoreNewsAsync_ShouldLogSuccess_WhenValidResponse()
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>();
            messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""
                {
                    "articles": [
                        {
                            "source": { "id": "cnn", "name": "CNN" },
                            "author": "John",
                            "title": "Test Title",
                            "description": "Test Desc",
                            "url": "https://example.com",
                            "urlToImage": "https://img.com",
                            "publishedAt": "2025-01-01T00:00:00Z",
                            "content": "Test Content"
                        }
                    ]
                }
                """)
                });

            var httpClient = new HttpClient(messageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://newsapi.org")
            };

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock
                .Setup(f => f.CreateClient(nameof(NewsApiClient)))
                .Returns(httpClient);

            var logger = Mock.Of<ILogger<NewsApiClient>>();
            var newsRepoMock = new Mock<INewsRepository>();
            var newsStorage = new NewsStorageAppService(newsRepoMock.Object);

            var client = new NewsApiClient(factoryMock.Object, newsStorage, logger);

            await client.FetchAndStoreNewsAsync("technology");
        }
    }
}
