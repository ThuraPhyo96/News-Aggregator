using MongoDB.Driver;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Models;
using NewsAggregator.Infrastructure.Persistence.MongoDb;

namespace NewsAggregator.Infrastructure.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly IMongoCollection<ArticleDocument> _articelCollection;

        public NewsRepository(IMongoDatabase database)
        {
            _articelCollection = database.GetCollection<ArticleDocument>("News");
        }

        public async Task<List<Article>> GetAllAsync()
        {
            var newsDocs = await _articelCollection.Find(_ => true).ToListAsync();
            return newsDocs.Select(doc => doc.ToDomain()).ToList();
        }

        public async Task<Article?> GetNewsByIdAsync(string id)
        {
            var article = await _articelCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
            return article.ToDomain();
        }

        public async Task AddAsync(Article news)
        {
            var newsDoc = ArticleDocument.FromDomain(news);
            await _articelCollection.InsertOneAsync(newsDoc);
        }
    }
}
