using FluentAssertions;
using Moq;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Helpers;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Tests.Services
{
    public class UserAppServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly IUserAppService _userAppService;

        public UserAppServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _userAppService = new UserAppService(_userRepoMock.Object);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnDto_WhenValidUsername()
        {
            // Arrange
            string username = "admin";

            User obj = new(Guid.NewGuid().ToString(), username, "123456");

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(obj);

            // Act
            var result = await _userAppService.GetByUsername(username);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data!.Username.Should().Be(username);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetByUsername_ShouldReturnEmptyOrNull_WhenUsernameIsNullOrEmpty(string username)
        {
            // Arrange is in InlineData

            // Act
            var result = await _userAppService.GetByUsername(username);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("User name can not be null or empty.");
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnNotFound_WhenNotExistingUsername()
        {
            // Arrange
            string username = "67eeac692d3c4efa81680200";

            _userRepoMock
               .Setup(x => x.GetByUsernameAsync(username))
               .ReturnsAsync((User?)null);

            // Act
            var result = await _userAppService.GetByUsername(username);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage!.Should().Contain("Not found!");
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnError_WhenRepositoryThrows()
        {
            // Arrange
            string username = "67eeac692d3c4efa816802ff";

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ThrowsAsync(new Exception("Database unreachable"));

            // Act
            var result = await _userAppService.GetByUsername(username);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred: Database unreachable");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnDto_WhenValidRequest()
        {
            // Arrange
            User user = new("67eeac692d3c4efa816802ff", "John", "123456");

            _userRepoMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(user);

            CreateUserDto input = new()
            {
                Username = "John",
                Password = "123456"
            };

            // Act
            var result = await _userAppService.CreateUser(input);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Username.Should().Be("John");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailed_WhenNullRequest()
        {
            // Act
            var result = await _userAppService.CreateUser(null);

            result.Success.Should().BeFalse();
            result.Data?.Should().Be("User can not be null.");
        }

        [Theory]
        [InlineData(null, "123456")]
        [InlineData("John", null)]
        public void CreateUser_ShouldBeInvalid_WhenRequiredFieldsAreMissing(string? username, string? password)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = username,
                Password = password
            };

            // Act
            var isValid = ValidationHelper.TryValidate(dto, out var validationResults);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(" ", "123456")]
        [InlineData("John", " ")]
        public async Task CreateUser_ShouldReturnFailed_WhenRequiredFieldsAreWhitespace(string? username, string? password)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _userAppService.CreateUser(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Username and Password cannot be empty or whitespace.");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailed_WhenAddAsyncNull()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = "John",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User?)null!);

            // Act
            var result = await _userAppService.CreateUser(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Failed to save user.");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnFailed_WhenExceptionThrown()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = "John",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _userAppService.CreateUser(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
            result.ErrorMessage.Should().Contain("Database connection failed");
        }

        [Fact]
        public async Task GetToken_ShouldReturnString_WhenValidRequest()
        {
            // Arrange
            User user = new("67eeac692d3c4efa816802ff", "John", "123456");

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync("John"))
                .ReturnsAsync(user);

            _userRepoMock
                .Setup(x => x.VerifyUser("123456", "123456"))
                .Returns(true);

            _userRepoMock
              .Setup(x => x.GetToken("John"))
              .ReturnsAsync("eIuhaihfruhaughuo.poiil.oiiejnvni988+9");

            LoginUserDto input = new()
            {
                Username = "John",
                Password = "123456"
            };

            // Act
            var result = await _userAppService.GetToken(input);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetToken_ShouldReturnFailed_WhenNullRequest()
        {
            // Act
            var result = await _userAppService.GetToken(null!);

            result.Success.Should().BeFalse();
            result.Data?.Should().Be("Login user can not be null.");
        }

        [Theory]
        [InlineData(null, "123456")]
        [InlineData("John", null)]
        public void GetToken_ShouldBeInvalid_WhenRequiredFieldsAreMissing(string? username, string? password)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = username,
                Password = password
            };

            // Act
            var isValid = ValidationHelper.TryValidate(dto, out var validationResults);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(" ", "123456")]
        [InlineData("John", " ")]
        public async Task GetToken_ShouldReturnFailed_WhenRequiredFieldsAreWhitespace(string? username, string? password)
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _userAppService.CreateUser(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Username and Password cannot be empty or whitespace.");
        }

        [Fact]
        public async Task GetToken_ShouldReturnInvalidCredentials_WhenUserIsNull()
        {
            // Arrange
            var dto = new LoginUserDto
            {
                Username = "John",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync("John"))
                .ReturnsAsync((User?)null!);

            // Act
            var result = await _userAppService.GetToken(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid credentials.");
        }

        [Fact]
        public async Task GetToken_ShouldReturnInvalidCredentials_WhenWrongPassword()
        {
            // Arrange
            User user = new("67eeac692d3c4efa816802ff", "John", "123456");

            var dto = new LoginUserDto
            {
                Username = "John",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync("John"))
                .ReturnsAsync(user);

            _userRepoMock
                .Setup(x => x.VerifyUser("123456", "1234"))
                .Returns(false);

            // Act
            var result = await _userAppService.GetToken(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid credentials.");
        }

        [Fact]
        public async Task GetToken_ShouldReturnEmpty_WhenGetTokenError()
        {
            // Arrange
            User user = new("67eeac692d3c4efa816802ff", "John", "123456");

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync("John"))
                .ReturnsAsync(user);

            _userRepoMock
                .Setup(x => x.VerifyUser("123456", "123456"))
                .Returns(true);

            _userRepoMock
              .Setup(x => x.GetToken("John"))
              .ReturnsAsync(string.Empty);

            var dto = new LoginUserDto
            {
                Username = "John",
                Password = "123456"
            };

            // Act
            var result = await _userAppService.GetToken(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GetToken_ShouldReturnFailed_WhenExceptionThrown()
        {
            // Arrange
            User user = new("67eeac692d3c4efa816802ff", "John", "123456");

            _userRepoMock
                .Setup(x => x.GetByUsernameAsync("John"))
                .ReturnsAsync(user);

            _userRepoMock
                .Setup(x => x.VerifyUser("123456", "123456"))
                .Returns(true);

            _userRepoMock
              .Setup(x => x.GetToken("John"))
              .ThrowsAsync(new Exception("Database connection failed"));

            var dto = new LoginUserDto
            {
                Username = "John",
                Password = "123456"
            };

            // Act
            var result = await _userAppService.GetToken(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
            result.ErrorMessage.Should().Contain("Database connection failed");
        }
    }
}
