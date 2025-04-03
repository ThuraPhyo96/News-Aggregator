using System.Text.Json.Serialization;

namespace NewsAggregator.Domain.Models
{
    public class Article
    {
        public string? Id { get; private set; }

        [JsonPropertyName("source")]
        public Source? Source { get; private set; }

        [JsonPropertyName("author")]
        public string? Author { get; private set; }

        [JsonPropertyName("title")]
        public string? Title { get; private set; }

        [JsonPropertyName("description")]
        public string? Description { get; private set; }

        [JsonPropertyName("url")]
        public string? Url { get; private set; }

        [JsonPropertyName("urlToImage")]
        public string? UrlToImage { get; private set; }

        [JsonPropertyName("publishedAt")]
        public DateTime? PublishedAt { get; private set; }

        [JsonPropertyName("content")]
        public string? Content { get; private set; }

        public Article()
        {

        }

        [JsonConstructor]
        public Article(Source? source, string? author, string? title, string? description, string? url, string? urlToImage, DateTime? publishedAt, string? content)
        {
            Source = source is null ? new Source() : source;
            Author = string.IsNullOrEmpty(author) ? "Unknown Author" : author;
            Title = string.IsNullOrEmpty(title) ? "Unknown Title" : title;
            Description = string.IsNullOrEmpty(description) ? "Unknown Description" : description;
            Url = string.IsNullOrEmpty(url) ? "Unknown url" : url;
            UrlToImage = string.IsNullOrEmpty(urlToImage) ? "Unknown urlToImage" : urlToImage;
            PublishedAt = publishedAt;
            Content = string.IsNullOrEmpty(content) ? "Unknown Content" : content;
        }
    }
}
