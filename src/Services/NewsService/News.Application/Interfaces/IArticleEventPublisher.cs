using NewsAggregator.Contracts.Events;

namespace News.Application.Interfaces
{
    public interface IArticleEventPublisher
    {
        void PublishArticlePublished(ArticlePublishedEvent articleEvent);
    }
}
