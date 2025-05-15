using Microsoft.Extensions.Configuration;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Events;
using NewsAggregator.Infrastructure.Helpers;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NewsAggregator.Infrastructure.Messaging
{
    public class ArticleEventPublisher : IArticleEventPublisher
    {
        private readonly string _queueName;
        private readonly IConfiguration _config;

        public ArticleEventPublisher(IConfiguration config)
        {
            _config = config;
            var env = Environment.GetEnvironmentVariable("RABBITMQ_ENV")! ?? _config["RabbitMq:Environment"];
            _queueName = RabbitMqQueueNames.NewsPublished(env!);
        }

        public void PublishArticlePublished(ArticlePublishedEvent newsEvent)
        {
            var uri = Environment.GetEnvironmentVariable("RABBITMQ_URI")! ?? _config["RabbitMq:Uri"];
            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri!),
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newsEvent));
            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish("", _queueName, props, body);
        }
    }
}
