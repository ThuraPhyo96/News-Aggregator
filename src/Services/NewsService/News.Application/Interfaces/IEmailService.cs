using NewsAggregator.Contracts.Events;

namespace News.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendNewsNotificationAsync(ArticlePublishedEvent news);
    }
}
