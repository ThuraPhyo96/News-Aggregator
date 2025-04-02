namespace NewsAggregator.Domain.Models
{
    public class Article
    {
        public string? Id { get; private set; }
        public Source? Source { get; private set; }
        public string? Author { get; private set; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }
        public string? Url { get; private set; }
        public string? UrlToImage { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public string? Content { get; private set; }

        public Article()
        {
                
        }

        public Article(string? id, Source? source, string? author, string? title, string? description, string? url, string? urlToImage, DateTime? publishedAt, string? content)
        {
            Id = id;
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
}
