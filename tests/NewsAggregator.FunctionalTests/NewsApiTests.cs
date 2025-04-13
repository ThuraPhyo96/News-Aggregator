using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NewsAggregator.FunctionalTests.TestDoubles;
using System.Net;
using System.Text;
using System.Text.Json;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Infrastructure.HttpClients;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;

namespace NewsAggregator.FunctionalTests
{
    public class NewsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public NewsApiTests(WebApplicationFactory<Program> factory)
        {
            factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<INewsRepository>();
                services.RemoveAll<NewsApiClient>();
                services.RemoveAll<INewsAppService>();
                services.RemoveAll<NewsStorageAppService>();

                services.AddScoped<INewsAppService, NewsAppService>();
                services.AddScoped<FailingArticleRepository>();
                services.AddScoped<INewsRepository, InMemoryNewsRepository>();
            });

            _factory = factory;
        }

        [Fact]
        public async Task GetNews_ShouldReturnOK()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/news");

            //var content = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetNewsById_ValidId_ShouldReturnOK()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/news/67eeac692d3c4efa816802ff");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetNewsById_InvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/news/0");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetNewsById_NoExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/news/67eeac742d3c4efa81680301");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateNews_ShouldReturnCreated_WhenValidArticlePosted()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newArticle = new
            {
                Source = new
                {
                    Id = "custom-id",
                    Name = "BBC"
                },
                Author = "Test Author",
                Title = "Test Article",
                Description = "Test Description",
                Url = "https://example.com",
                UrlToImage = "https://example.com/image.jpg",
                PublishedAt = DateTime.UtcNow,
                Content = "Test content"
            };

            var content = new StringContent(JsonSerializer.Serialize(newArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/news", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateNews_ShouldReturnBadRequest_WhenInvalidArticlePosted()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidArticle = new { };

            var content = new StringContent(JsonSerializer.Serialize(invalidArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/news", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnNoContent_WhenValidArticle()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "67eeac692d3c4efa816802ff";
            var updateArticle = new
            {
                Source = new
                {
                    Id = "Updated custom-id",
                    Name = "Updated BBC"
                },
                Author = "Updated Test Author",
                Title = "Updated Test Article",
                Description = "Updated Test Description",
                Url = "https://example.com/updatednews",
                UrlToImage = "https://example.com/updatednews/image.jpg",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Test content"
            };

            var content = new StringContent(JsonSerializer.Serialize(updateArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/news/{id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateNews_ShoulReturnBadRequest_WhenInvalidId()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "6";
            var updateArticle = new
            {
                Source = new
                {
                    Id = "Updated custom-id",
                    Name = "Updated BBC"
                },
                Author = "Updated Test Author",
                Title = "Updated Test Article",
                Description = "Updated Test Description",
                Url = "https://example.com/updatednews",
                UrlToImage = "https://example.com/updatednews/image.jpg",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Test content"
            };

            var content = new StringContent(JsonSerializer.Serialize(updateArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/news/{id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnNotFound_WhenNoExistingId()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "67eeac692d3c4efa81680200";
            var updateArticle = new
            {
                Source = new
                {
                    Id = "Updated custom-id",
                    Name = "Updated BBC"
                },
                Author = "Updated Test Author",
                Title = "Updated Test Article",
                Description = "Updated Test Description",
                Url = "https://example.com/updatednews",
                UrlToImage = "https://example.com/updatednews/image.jpg",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Test content"
            };

            var content = new StringContent(JsonSerializer.Serialize(updateArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/news/{id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnInternalServer_WhenUpdateFailed()
        {
            // Arrange
            var factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<INewsRepository>();
                services.RemoveAll<NewsApiClient>();
                services.RemoveAll<INewsAppService>();
                services.RemoveAll<NewsStorageAppService>();

                services.AddScoped<FailingArticleRepository>();
            });
            var client = factory.CreateClient();
            string id = "67eeac692d3c4efa81680200";
            var updateArticle = new
            {
                Source = new
                {
                    Id = "Updated custom-id",
                    Name = "Updated BBC"
                },
                Author = "Updated Test Author",
                Title = "Updated Test Article",
                Description = "Updated Test Description",
                Url = "https://example.com/updatednews",
                UrlToImage = "https://example.com/updatednews/image.jpg",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Test content"
            };

            var content = new StringContent(JsonSerializer.Serialize(updateArticle), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/api/news/{id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnNoContent_WhenValidId()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "67eeac692d3c4efa816802ff";

            // Act
            var response = await client.DeleteAsync($"/api/news/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnBadRequest_WhenInvalidId()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "5";

            // Act
            var response = await client.DeleteAsync($"/api/news/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnNotFound_WhenNoExistingId()
        {
            // Arrange
            var client = _factory.CreateClient();
            string id = "67eeac692d3c4efa81680200";

            // Act
            var response = await client.DeleteAsync($"/api/news/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnInternalServer_WhenDeleteFailed()
        {
            try
            {
                // Arrange
                var factory = new CustomWebAppFactory(services =>
                {
                    services.RemoveAll<INewsRepository>();
                    services.RemoveAll<NewsApiClient>();
                    services.RemoveAll<INewsAppService>();
                    services.RemoveAll<NewsStorageAppService>();

                    services.AddScoped<FailingArticleRepository>();
                });

                var client = factory.CreateClient();
                string id = "67eeac692d3c4efa81680200";

                // Act
                var response = await client.DeleteAsync($"/api/news/{id}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
