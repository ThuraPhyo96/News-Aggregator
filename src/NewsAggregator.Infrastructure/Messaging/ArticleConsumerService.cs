using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NewsAggregator.Application.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NewsAggregator.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;
using NewsAggregator.Infrastructure.Helpers;

namespace NewsAggregator.Infrastructure.Messaging
{
    public class ArticleConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _queueName;
        private readonly IConfiguration _config;
        private readonly ILogger<ArticleConsumerService> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public ArticleConsumerService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<ArticleConsumerService> logger, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
            _loggerFactory = loggerFactory;
            var env = Environment.GetEnvironmentVariable("RABBITMQ_ENV")! ?? _config["RabbitMq:Environment"];
            _queueName = RabbitMqQueueNames.NewsPublished(env!);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ArticleConsumerService...");
            var uri = Environment.GetEnvironmentVariable("RABBITMQ_URI")! ?? _config["RabbitMq:Uri"];
            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri!),
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var configuratorLogger = _loggerFactory.CreateLogger<RabbitMqQueueConfigurator>();
            var configurator = new RabbitMqQueueConfigurator(_channel, configuratorLogger, _config);
            configurator.DeclareAllQueues();

            _logger.LogInformation("RabbitMQ connection established and queues declared.");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized.");
                return Task.CompletedTask;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancellation requested. Skipping message.");
                    return;
                }

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var article = JsonSerializer.Deserialize<ArticlePublishedEvent>(json);

                    if (article == null)
                    {
                        _logger.LogWarning("Received null or malformed ArticlePublishedEvent.");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    if (article.Title!.Contains("fail", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Simulated failure for retry");
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendNewsNotificationAsync(article);

                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Processed article: {Title}", article.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message. Sending to retry queue.");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("ArticleConsumerService started consuming messages.");
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping ArticleConsumerService...");
            _channel?.Close();
            _connection?.Close();
            _logger.LogInformation("RabbitMQ connection closed.");
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
