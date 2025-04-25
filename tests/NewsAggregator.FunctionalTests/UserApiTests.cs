using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.FunctionalTests.TestDoubles;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregator.FunctionalTests
{
    public class UserApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UserApiTests(WebApplicationFactory<Program> factory)
        {
            factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IUserAppService>();

                services.AddScoped<IUserAppService, UserAppService>();
                services.AddScoped<IUserRepository, InMemoryUserRepository>();
            });

            _factory = factory;
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnOK_WhenValidUsername()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth?username=admin");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnOk_WhenValidUsernameWithDifferentCase()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/auth?username=ADMIN");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnNotFound_WhenNoExistingUsername_()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth?username=Smith");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnBadRequest_WhenEmptyUsername()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth?username= ");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnBadRequest_WhenInvalidCharacters()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth?username=@@@###");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnInternalServer_WhenFailed()
        {
            // Arrange
            var factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IUserAppService>();

                services.AddScoped<FailingUserRepository>();
            });
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/auth?username=admin");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreated_WhenValidUserPosted()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = new
            {
                Username = "Willie",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "123456")]
        [InlineData("admin", "")]
        public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidUserPosted(string username, string password)
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidUser = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidUser), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenDuplicateUsername()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidUser = new
            {
                Username = "admin",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidUser), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidCharactersUsername()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidUser = new
            {
                Username = "@@@###",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidUser), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnInternalServer_WhenFailed()
        {
            // Arrange
            var factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IUserAppService>();

                services.AddScoped<FailingArticleRepository>();
            });
            var client = factory.CreateClient();

            var invalidUser = new
            {
                Username = "admin",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidUser), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnOK_WhenValidUserPosted()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = new
            {
                Username = "admin",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnOK_WhenUsernameIsDifferentCase()
        {
            var client = _factory.CreateClient();
            var user = new
            {
                Username = "AdMiN",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnBadRequest_WhenInvalidUserPosted()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidUser = new { };

            var content = new StringContent(JsonSerializer.Serialize(invalidUser), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
        {
            var client = _factory.CreateClient();
            var user = new
            {
                Username = "admin",
                Password = "978783"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnBadRequest_WhenUsernameNotExists()
        {
            var client = _factory.CreateClient();
            var user = new
            {
                Username = "Smith",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnBadRequest_WhenUsernameMissing()
        {
            var client = _factory.CreateClient();
            var user = new { Password = "123456" };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnBadRequest_WhenPasswordMissing()
        {
            var client = _factory.CreateClient();
            var user = new { Username = "admin" };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnInternalServer_WhenFailed()
        {
            // Arrange
            var factory = new CustomWebAppFactory(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IUserAppService>();

                services.AddScoped<FailingUserRepository>();
            });
            var client = factory.CreateClient();
            var user = new
            {
                Username = "admin",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
