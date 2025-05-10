using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NewsAggregator.Application.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NewsAggregator.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace NewsAggregator.Infrastructure.Messaging
{
    public class ArticleConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _queueName = "news-published-queue";
        private readonly IConfiguration _config;

        public ArticleConsumerService(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var uri = Environment.GetEnvironmentVariable("RABBITMQ_URI")! ?? _config["RabbitMq:Uri"];
            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri!),
                DispatchConsumersAsync = true
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var article = JsonSerializer.Deserialize<ArticlePublishedEvent>(json);
                if (article != null)
                {
                    await emailService.SendNewsNotificationAsync(article);
                }
                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            // Keep running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
