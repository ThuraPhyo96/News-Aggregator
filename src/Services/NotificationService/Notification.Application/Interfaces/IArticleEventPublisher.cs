using NewsAggregator.Contracts.Events;

namespace Notification.Application.Interfaces
{
    public interface IArticleEventPublisher
    {
        void PublishArticlePublished(ArticlePublishedEvent articleEvent);
    }
}
