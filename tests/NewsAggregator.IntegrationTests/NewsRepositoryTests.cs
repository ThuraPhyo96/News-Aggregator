using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;
using NewsAggregator.Domain.Models;
using NewsAggregator.Infrastructure.Repositories;

namespace NewsAggregator.IntegrationTests
{
    public class NewsRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _mongoDbRunner;
        private readonly IMongoDatabase _database;
        private readonly NewsRepository _newsRepository;

        public NewsRepositoryTests()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoDbRunner.ConnectionString);
            _database = client.GetDatabase("TestMongoDB");

            _newsRepository = new NewsRepository(_database);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCount1_WhenGetAllArticles()
        {
            // Arrange
            var article = new Article(
                source: new Source("cnn", "CNN"),
                author: "John",
                title: "Test Title",
                description: "Desc",
                url: "url",
                urlToImage: "image",
                publishedAt: DateTime.UtcNow,
                content: "Content"
            );

            // Act
            await _newsRepository.AddAsync(article);
            var result = await _newsRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetNewsByIdAsync_ShouldReturnArticle_WhenGetById()
        {
            // Arrange
            var article = new Article(
                source: new Source("cnn", "CNN"),
                author: "John",
                title: "Test Title",
                description: "Desc",
                url: "url",
                urlToImage: "image",
                publishedAt: DateTime.UtcNow,
                content: "Content"
            );

            // Act
            var addedArticle = await _newsRepository.AddAsync(article);
            var result = await _newsRepository.GetNewsByIdAsync(addedArticle.Id!);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Author.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetNewsByIdAsync_ShouldReturnNull_WhenArticleNotFound()
        {
            // Act
            var result = await _newsRepository.GetNewsByIdAsync("67eeac692d3c4efa816802ff");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldInsertArticle()
        {
            // Arrange
            var article = new Article(
                source: new Source("cnn", "CNN"),
                author: "John",
                title: "Test Title",
                description: "Desc",
                url: "url",
                urlToImage: "image",
                publishedAt: DateTime.UtcNow,
                content: "Content"
            );

            // Act
            var result = await _newsRepository.AddAsync(article);

            // Assert
            result.Should().NotBeNull();
            result.Author.Should().Be("John");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateArticle()
        {
            // Arrange
            var article = new Article(
                source: new Source("cnn", "CNN"),
                author: "John",
                title: "Test Title",
                description: "Desc",
                url: "url",
                urlToImage: "image",
                publishedAt: DateTime.UtcNow,
                content: "Content"
            );

            var updateArticle = new Article(
               source: new Source("cnn", "CNN"),
               author: "John",
               title: "Updated Test Title",
               description: "Desc",
               url: "url",
               urlToImage: "image",
               publishedAt: DateTime.UtcNow,
               content: "Content"
            );

            // Act
            var addedArticle = await _newsRepository.AddAsync(article);
            var result = await _newsRepository.UpdateAsync(addedArticle.Id!, updateArticle);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn1_WhenArticleDeleted()
        {
            // Arrange
            var article = new Article(
                source: new Source("cnn", "CNN"),
                author: "John",
                title: "Test Title",
                description: "Desc",
                url: "url",
                urlToImage: "image",
                publishedAt: DateTime.UtcNow,
                content: "Content"
            );

            // Act
            var addedArticle = await _newsRepository.AddAsync(article);
            var result = await _newsRepository.DeleteAsync(addedArticle.Id!);

            // Assert
            result.Should().Be(1);
        }

        public void Dispose()
        {
            _mongoDbRunner.Dispose();
        }
    }
}
