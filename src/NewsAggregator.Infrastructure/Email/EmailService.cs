using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Events;
using NewsAggregator.Infrastructure.Settings;

namespace NewsAggregator.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly SmtpSettings _settings;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _settings = new SmtpSettings
            {
                Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _config["Smtp:Host"]!,
                Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : Convert.ToInt32(_config["Smtp:Port"]!),
                User = Environment.GetEnvironmentVariable("SMTP_USER") ?? _config["Smtp:User"]!,
                Pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _config["Smtp:Password"]!,
                From = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Smtp:From"]!
            };
        }

        public async Task SendNewsNotificationAsync(ArticlePublishedEvent news)
        {
            var subscribers = new[] { "thura.bgo.war.0502@gmail.com" };

            foreach (var email in subscribers)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("News Aggregator", _settings.From));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = $"New Article: {news.Title}";
                message.Body = new TextPart("plain")
                {
                    Text = $"{news.Title}\n\n{news.Description}"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.User, _settings.Pass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
