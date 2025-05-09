using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NewsAggregator.Domain.Events;
using NewsAggregator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace NewsAggregator.Worker.Services
{
    public class ArticlePublishedConsumer
    {
        private readonly IEmailService _emailService;
        private readonly string _queueName = "news-published-queue";
        private readonly IConfiguration _config;

        public ArticlePublishedConsumer(IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _config = config;
        }

        public void Start()
        {
            var uri = Environment.GetEnvironmentVariable("RABBITMQ_URI")! ?? _config["RabbitMq:Uri"];
            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri!),
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, e) =>
            {
                var json = Encoding.UTF8.GetString(e.Body.ToArray());
                var article = JsonSerializer.Deserialize<ArticlePublishedEvent>(json);

                await _emailService.SendNewsNotificationAsync(article!);
                channel.BasicAck(e.DeliveryTag, false);
            };

            channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Worker running. Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
