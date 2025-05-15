namespace NewsAggregator.Infrastructure.Helpers
{
    public static class RabbitMqQueueNames
    {
        public static string GetQueue(string baseName, string environment)
        {
            return $"{baseName}-{environment}";
        }

        public static string NewsPublished(string environment) => GetQueue("news-published-queue", environment);
        public static string Retry1(string environment) => GetQueue("retry-queue-1", environment);
        public static string Retry2(string environment) => GetQueue("retry-queue-2", environment);
        public static string DeadLetter(string environment) => GetQueue("dead-letter-queue", environment);
    }
}
