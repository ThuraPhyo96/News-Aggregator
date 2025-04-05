using NewsAggregator.Application.DTOs;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Mappers
{
    public static class ArticleMapper
    {
        public static ArticleDto? ToDto(Article article)
        {
            if (article == null) return null;

            return new ArticleDto
            {
                Id = article.Id,
                Source = article.Source != null ? new SourceDto
                {
                    Id = article.Source.Id,
                    Name = article.Source.Name
                } : null,
                Author = article.Author,
                Title = article.Title,
                Description = article.Description,
                Url = article.Url,
                UrlToImage = article.UrlToImage,
                PublishedAt = article.PublishedAt,
                Content = article.Content
            };
        }

        public static List<ArticleDto> ToDtoList(List<Article> articles)
        {
            if (articles == null) return [];

            return articles.Select(ToDto).ToList()!;
        }

        public static Article? ToEntity(CreateArticleDto article)
        {
            if (article == null) return null;

            return new Article(
                SourceMapper.ToEntity(article.Source!),
                article.Author,
                article.Title,
                article.Description,
                article.Url,
                article.UrlToImage,
                article.PublishedAt,
                article.Content);
        }

        public static Article? ToEntity(UpdateArticleDto article)
        {
            if (article == null) return null;

            return new Article(
                SourceMapper.ToEntity(article.Source!),
                article.Author,
                article.Title,
                article.Description,
                article.Url,
                article.UrlToImage,
                article.PublishedAt,
                article.Content);
        }
    }
}
