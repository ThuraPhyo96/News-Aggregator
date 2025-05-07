using NewsAggregator.Domain.Events;

namespace NewsAggregator.Application.Interfaces
{
    public interface IArticleEventPublisher
    {
        void PublishArticlePublished(ArticlePublishedEvent articleEvent);
    }
}
