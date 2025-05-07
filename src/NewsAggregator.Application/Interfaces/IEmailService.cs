using NewsAggregator.Domain.Events;

namespace NewsAggregator.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendNewsNotificationAsync(ArticlePublishedEvent news);
    }
}
