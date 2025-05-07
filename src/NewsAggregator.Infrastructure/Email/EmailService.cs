using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Events;

namespace NewsAggregator.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        public Task SendNewsNotificationAsync(ArticlePublishedEvent news)
        {
            // Fetch subscribers (mocked)
            var subscribers = new[] { "user1@example.com", "user2@example.com" };

            foreach (var email in subscribers)
            {
                Console.WriteLine($"[Email] Sent news '{news.Title}' to {email}");
                // Use MailKit or SMTP to actually send
            }

            return Task.CompletedTask;
        }
    }
}
