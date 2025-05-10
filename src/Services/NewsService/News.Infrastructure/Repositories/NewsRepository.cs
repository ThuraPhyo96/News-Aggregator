using MongoDB.Driver;
using News.Domain.Interfaces;
using News.Domain.Models;
using News.Infrastructure.Persistence.MongoDb;

namespace News.Infrastructure.Repositories
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
            try
            {
                var article = await _articelCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
                if (article is null)
                    return null;

                return article.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Article> AddAsync(Article news)
        {
            try
            {
                var newsDoc = ArticleDocument.FromDomain(news);
                await _articelCollection.InsertOneAsync(newsDoc);
                return newsDoc.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<long> UpdateAsync(string id, Article article)
        {
            try
            {
                var existingObj = await _articelCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
                if (existingObj is null)
                    return 0;

                var updateDoc = ArticleDocument.FromDomain(article);
                updateDoc.Id = id;

                var result = await _articelCollection.ReplaceOneAsync(
                    doc => doc.Id == id,
                    updateDoc
                );

                return result.ModifiedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<long> DeleteAsync(string id)
        {
            try
            {
                var existingObj = await _articelCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
                if (existingObj is null)
                    return 0;

                var result = await _articelCollection.DeleteOneAsync(doc => doc.Id == id);
                return result.DeletedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
