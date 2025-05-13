using NewsAggregator.Contracts.Events;

namespace Notification.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendNewsNotificationAsync(ArticlePublishedEvent news);
    }
}
