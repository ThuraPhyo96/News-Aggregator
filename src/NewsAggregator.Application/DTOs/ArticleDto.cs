using NewsAggregator.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.Application.DTOs
{
    public class ArticleDto
    {
        public string? Id { get; set; }
        public SourceDto? Source { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }
    }

    public class CreateArticleDto
    {
        public SourceDto? Source { get; set; }

        [Required]
        public string? Author { get; set; }

        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }

        [Required]
        public string? Content { get; set; }

        public CreateArticleDto()
        {

        }

        public CreateArticleDto(SourceDto? source, string? author, string? title, string? description, string? url, string? urlToImage, DateTime? publishedAt, string? content)
        {
            Source = source;
            Author = author;
            Title = title;
            Description = description;
            Url = url;
            UrlToImage = urlToImage;
            PublishedAt = publishedAt;
            Content = content;
        }
    }

    public class UpdateArticleDto
    {
        public SourceDto? Source { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }
    }
}
