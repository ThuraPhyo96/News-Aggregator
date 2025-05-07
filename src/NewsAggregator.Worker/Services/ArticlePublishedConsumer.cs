using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NewsAggregator.Domain.Events;
using NewsAggregator.Application.Interfaces;

namespace NewsAggregator.Worker.Services
{
    public class ArticlePublishedConsumer
    {
        private readonly IEmailService _emailService;
        private readonly string _queueName = "news-published-queue";

        public ArticlePublishedConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public void Start()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
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
