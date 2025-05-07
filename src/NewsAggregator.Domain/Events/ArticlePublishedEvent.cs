namespace NewsAggregator.Domain.Events
{
    public class ArticlePublishedEvent
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
