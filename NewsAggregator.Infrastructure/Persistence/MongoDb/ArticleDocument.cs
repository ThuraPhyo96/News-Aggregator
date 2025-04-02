using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Infrastructure.Persistence.MongoDb
{
    public class ArticleDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public Source? Source { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }

        // Convert from Domain Entity to Mongo Document
        public static ArticleDocument FromDomain(Article article)
        {
            return new ArticleDocument
            {
                Id = article.Id,
                Source = article.Source,
                Author = article.Author,
                Title = article.Title,
                Description = article.Description,
                Url = article.Url,
                UrlToImage = article.UrlToImage,
                PublishedAt = article.PublishedAt,
                Content = article.Content
            };
        }

        // Convert from Mongo Document to Domain Entity
        public Article ToDomain()
        {
            return new Article(Id, Source, Author, Title, Description, Url, UrlToImage, PublishedAt, Content);
        }
    }
}
