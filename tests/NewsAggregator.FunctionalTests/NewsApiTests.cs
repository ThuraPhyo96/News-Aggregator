using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NewsAggregator.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NewsAggregator.FunctionalTests
{
    public class NewsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public NewsApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetNews_ShouldReturnOK()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/news");

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
    }
}
