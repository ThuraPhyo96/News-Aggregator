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
    public class NewsAppServiceTests
    {
        private readonly Mock<INewsRepository> _newsRepoMock;
        private readonly NewsAppService _newsAppService;
        private readonly Mock<IArticleEventPublisher> _articleEventPublisherMock;

        public NewsAppServiceTests()
        {
            _newsRepoMock = new Mock<INewsRepository>();
            _articleEventPublisherMock = new Mock<IArticleEventPublisher>();
            _newsAppService = new NewsAppService(_newsRepoMock.Object, _articleEventPublisherMock.Object);
        }

        [Fact]
        public async Task GetAllNews_ShouldReturnMappedDtos()
        {
            // Arrange
            var articles = new List<Article>
            {
                new("1", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content")
            };

            _newsRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(articles);

            // Act
            var result = await _newsAppService.GetAllNews();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result.First().Title.Should().Contain("Title");
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnDto_WhenValidId()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            Article article = new(id, new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            // Act
            var result = await _newsAppService.GetNewsById(id);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(id);
            result.Data!.Content.Should().Contain("Content");
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnInvalidId_WhenInvalidId()
        {
            // Arrange
            string id = "1";

            Article article = new(id, new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
               .Setup(x => x.GetNewsByIdAsync(id))
               .ReturnsAsync(article);

            // Act
            var result = await _newsAppService.GetNewsById(id);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage!.Should().Contain("Invalid ID format.");
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnNotFound_WhenNotExistingId()
        {
            // Arrange
            string id = "67eeac692d3c4efa81680200";

            _newsRepoMock
               .Setup(x => x.GetNewsByIdAsync(id))
               .ReturnsAsync((Article?)null);

            // Act
            var result = await _newsAppService.GetNewsById(id);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage!.Should().Contain("Not found!");
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnError_WhenRepositoryThrows()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ThrowsAsync(new Exception("Database unreachable"));

            // Act
            var result = await _newsAppService.GetNewsById(id);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred: Database unreachable");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetNewsById_ShouldReturnInvalidId_WhenIdIsNullOrEmpty(string id)
        {
            // Arrange is in InlineData

            // Act
            var result = await _newsAppService.GetNewsById(id);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid ID format.");
        }

        [Theory]
        [InlineData("5f4e")] // too short
        [InlineData("5f4e3ab2c1d5f7e6a8c9d0e1aa")] // too long
        [InlineData("ZZZZZZZZZZZZZZZZZZZZZZZZ")] // non-hex
        [InlineData("5f4e3ab2@#c1d5f7e6a8c9d0")] // special chars
        [InlineData("5f4e3ab2g1d5f7e6a8c9d0e")] // g is invalid in hex
        public async Task GetNewsById_ShouldReturnInvalidId_WhenIdIsMalformed(string id)
        {
            var result = await _newsAppService.GetNewsById(id);
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid ID format.");
        }

        [Fact]
        public async Task CreateArticle_ShouldReturnDto_WhenValidRequest()
        {
            // Arrange
            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.AddAsync(It.IsAny<Article>()))
                .ReturnsAsync(article);

            CreateArticleDto createArticle = new()
            {
                Source = new SourceDto
                {
                    Id = "cnn",
                    Name = "CNN"
                },
                Author = "John",
                Title = "Title",
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow,
                Content = "Content"
            };

            // Act
            var result = await _newsAppService.CreateArticle(createArticle);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Author.Should().Be("John");
        }

        [Fact]
        public async Task CreateArticle_ShouldReturnFailed_WhenNullRequest()
        {
            // Act
            var result = await _newsAppService.CreateArticle(null);

            result.Success.Should().BeFalse();
            result.Data?.Should().Be("Article is null.");
        }

        [Theory]
        [InlineData(null, "Title", "Content")]
        [InlineData("Author", null, "Content")]
        [InlineData("Author", "Title", null)]
        public void CreateArticle_ShouldBeInvalid_WhenRequiredFieldsAreMissing(string? author, string? title, string? content)
        {
            // Arrange
            var dto = new CreateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = author,
                Title = title,
                Content = content,
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow
            };

            // Act
            var isValid = ValidationHelper.TryValidate(dto, out var validationResults);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(" ", "Title", "Content")]
        [InlineData("John", " ", "Content")]
        [InlineData("John", "Title", " ")]
        public async Task CreateArticle_ShouldReturnFailed_WhenRequiredFieldsAreWhitespace(string? author, string? title, string? content)
        {
            // Arrange
            var dto = new CreateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = author,
                Title = title,
                Content = content,
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow
            };

            // Act
            var result = await _newsAppService.CreateArticle(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Author, Title, and Content cannot be empty or whitespace.");
        }

        [Fact]
        public async Task CreateArticle_ShouldReturnFailed_WhenAddAsyncFails()
        {
            // Arrange
            var dto = new CreateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "John",
                Title = "Title",
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow,
                Content = "Content"
            };

            _newsRepoMock
                .Setup(x => x.AddAsync(It.IsAny<Article>()))
                .ReturnsAsync((Article?)null!);

            // Act
            var result = await _newsAppService.CreateArticle(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Failed to save article");
        }

        [Fact]
        public async Task CreateArticle_ShouldReturnFailed_WhenExceptionThrown()
        {
            // Arrange
            var dto = new CreateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "John",
                Title = "Title",
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow,
                Content = "Content"
            };

            _newsRepoMock
                .Setup(x => x.AddAsync(It.IsAny<Article>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _newsAppService.CreateArticle(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
            result.ErrorMessage.Should().Contain("Database connection failed");
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnUpdatedCount_WhenValidRequest()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";
            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.UpdateAsync(id, It.IsAny<Article>()))
                .ReturnsAsync(1);

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Should().Be(1);
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnFailed_WhenIdIsNull()
        {
            // Arrange
            string id = null!;

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("ID cannot be empty or null.");
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnInvalidIdFormat_WhenInvalidId()
        {
            // Arrange
            string id = "1";

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("Invalid ID format.");
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnNotFound_WhenNotExistingId()
        {
            // Arrange
            string id = "67eeac692d3c4efa81680200";

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            _newsRepoMock
              .Setup(x => x.GetNewsByIdAsync(id))
              .ReturnsAsync((Article?)null);

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("Not found!");
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnFailed_WhenUpdateAsyncFails()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.UpdateAsync(id, It.IsAny<Article>()))
                .ReturnsAsync(0);

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage.Should().Be("Failed to update article");
        }

        [Theory]
        [InlineData(null, "Title", "Content")]
        [InlineData("Author", null, "Content")]
        [InlineData("Author", "Title", null)]
        public void UpdateArticle_ShouldBeInvalid_WhenRequiredFieldsAreMissing(string? author, string? title, string? content)
        {
            // Arrange
            var dto = new UpdateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = author,
                Title = title,
                Content = content,
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow
            };

            // Act
            var isValid = ValidationHelper.TryValidate(dto, out var validationResults);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(" ", "Title", "Content")]
        [InlineData("John", " ", "Content")]
        [InlineData("John", "Title", " ")]
        public async Task UpdateArticle_ShouldReturnFailed_WhenRequiredFieldsAreWhitespace(string? author, string? title, string? content)
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            var dto = new UpdateArticleDto
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = author,
                Title = title,
                Content = content,
                Description = "Desc",
                Url = "url",
                UrlToImage = "img",
                PublishedAt = DateTime.UtcNow
            };

            // Act
            var result = await _newsAppService.UpdateArticle(id, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Author, Title, and Content cannot be empty or whitespace.");
        }

        [Fact]
        public async Task UpdateArticle_ShouldReturnFailed_WhenExceptionThrown()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.UpdateAsync(id, It.IsAny<Article>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _newsAppService.UpdateArticle(id, updateArticle);

            // Assert
            result.Success.Should().BeFalse();
            result.Data.Should().Be(0);
            result.ErrorMessage.Should().Contain("An error occurred");
            result.ErrorMessage.Should().Contain("Database connection failed");
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnDeletedCount_WhenValidId()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";
            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.DeleteAsync(id))
                .ReturnsAsync(1);

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Should().Be(1);
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnNull_WhenIdIsNull()
        {
            // Arrange
            string id = null!;

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("ID cannot be empty or null.");
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnInvalidIdFormat_WhenInvalidId()
        {
            // Arrange
            string id = "1";

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("Invalid ID format.");
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnNotFound_WhenNotExistingId()
        {
            // Arrange
            string id = "67eeac692d3c4efa81680200";

            _newsRepoMock
              .Setup(x => x.GetNewsByIdAsync(id))
              .ReturnsAsync((Article?)null);

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage!.Should().Be("Not found!");
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnFailed_WhenDeleteAsyncFails()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.DeleteAsync(id))
                .ReturnsAsync(0);

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Data!.Should().Be(0);
            result.ErrorMessage.Should().Be("Failed to delete article");
        }

        [Fact]
        public async Task DeleteArticle_ShouldReturnFailed_WhenExceptionThrown()
        {
            // Arrange
            string id = "67eeac692d3c4efa816802ff";

            UpdateArticleDto updateArticle = new()
            {
                Source = new SourceDto { Id = "cnn", Name = "CNN" },
                Author = "Updated John",
                Title = "Updated Title",
                Description = "Updated Desc",
                Url = "Updated url",
                UrlToImage = "Updated img",
                PublishedAt = DateTime.UtcNow,
                Content = "Updated Content"
            };

            Article article = new("67eeac692d3c4efa816802ff", new Source("cnn", "CNN"), "John", "Title", "Desc", "url", "img", DateTime.UtcNow, "Content");

            _newsRepoMock
                .Setup(x => x.GetNewsByIdAsync(id))
                .ReturnsAsync(article);

            _newsRepoMock
                .Setup(x => x.DeleteAsync(id))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _newsAppService.DeleteArticle(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Data.Should().Be(0);
            result.ErrorMessage.Should().Contain("An error occurred");
            result.ErrorMessage.Should().Contain("Database connection failed");
        }
    }
}
