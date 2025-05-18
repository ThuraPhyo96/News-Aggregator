using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace NewsAggregator.Contracts.Helpers
{
    public class RabbitMqQueueConfigurator
    {
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqQueueConfigurator> _logger;
        private readonly string _queueName;
        private readonly string _retryQueue1;
        private readonly string _retryQueue2;
        private readonly string _deadLetterQueue;
        private readonly IConfiguration _config;

        public RabbitMqQueueConfigurator(IModel channel, ILogger<RabbitMqQueueConfigurator> logger, IConfiguration config)
        {
            _channel = channel;
            _logger = logger;
            _config = config;
            var env = Environment.GetEnvironmentVariable("RABBITMQ_ENV")! ?? _config["RabbitMq:Environment"];
            _queueName = RabbitMqQueueNames.NewsPublished(env!);
            _retryQueue1 = RabbitMqQueueNames.Retry1(env!);
            _retryQueue2 = RabbitMqQueueNames.Retry2(env!);
            _deadLetterQueue = RabbitMqQueueNames.DeadLetter(env!);
        }

        public void DeclareAllQueues()
        {
            try
            {
                // 1. Final dead-letter queue
                DeclareQueue(_deadLetterQueue, null);

                // 2. Retry queue 2 → goes to dead-letter-queue after failure
                DeclareQueue(_retryQueue2, arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = _deadLetterQueue,
                    ["x-message-ttl"] = 10000 // 10s
                });

                // 3. Retry queue 1 → goes to retry-queue-2
                DeclareQueue(_retryQueue1, arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = _retryQueue2,
                    ["x-message-ttl"] = 10000 // 10s
                });

                // 4. Main queue → DLX to retry-queue-1
                DeclareQueue(_queueName, arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = _retryQueue1
                });

                _logger.LogInformation("RabbitMQ queues declared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error declaring RabbitMQ queues.");
                throw;
            }
        }

        private void DeclareQueue(string baseName, IDictionary<string, object>? arguments)
        {
            var queueName = baseName;
            _logger.LogInformation("Declaring queue {QueueName} with arguments: {Args}", queueName, arguments);

            try
            {
                _channel.QueueDeclare(queue: queueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: arguments);
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex) when (ex.ShutdownReason.ReplyCode == 406)
            {
                _logger.LogWarning("Queue {QueueName} already exists with different properties. Skipping declaration. Consider cleaning or versioning queues.", queueName);
            }
        }
    }
}
