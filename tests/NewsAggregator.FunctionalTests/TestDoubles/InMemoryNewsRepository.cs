using MongoDB.Bson;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace NewsAggregator.FunctionalTests.TestDoubles
{
    public class InMemoryNewsRepository : INewsRepository
    {
        private readonly ConcurrentDictionary<string, Article> _articles = new();

        public InMemoryNewsRepository()
        {
            Source source = new("source-id", "CNN");

            Article article = new(
                "67eeac692d3c4efa816802ff",
                source,
                author: "Test Author",
                title: "Test Title",
                description: "Test Desc",
                url: "http://example.com",
                urlToImage: "http://example.com",
                publishedAt: DateTime.UtcNow,
                content: "Test content"
                );

            _articles[article.Id!] = article;
        }

        public Task<List<Article>> GetAllAsync()
        {
            return Task.FromResult(_articles.Values.ToList());
        }

        public Task<Article?> GetNewsByIdAsync(string id)
        {
            _articles.TryGetValue(id, out var article);
            return Task.FromResult(article);
        }

        public Task<Article> AddAsync(Article article)
        {
            var generatedId = ObjectId.GenerateNewId().ToString();

            // Use reflection to set the private Id property
            typeof(Article)
                .GetProperty(nameof(Article.Id), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?.SetValue(article, generatedId);

            _articles[article.Id!] = article; // Assuming _articles is a List<Article>

            return Task.FromResult<Article?>(article)!;
        }

        public Task<long> UpdateAsync(string id, Article article)
        {
            if (_articles.ContainsKey(id))
            {
                // Use reflection to set the private Id property
                typeof(Article)
                    .GetProperty(nameof(Article.Id), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.SetValue(article, id);

                _articles[id] = article;
                return Task.FromResult(1L);
            }
            return Task.FromResult(0L);
        }

        public Task<long> DeleteAsync(string id)
        {
            var success = _articles.TryRemove(id, out _);
            return Task.FromResult(success ? 1L : 0L);
        }
    }
}
