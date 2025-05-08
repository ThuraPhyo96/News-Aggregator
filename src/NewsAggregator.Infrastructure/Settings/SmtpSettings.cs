namespace NewsAggregator.Infrastructure.Settings
{
    public class SmtpSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public string? User { get; set; } = default!;
        public string? Pass { get; set; } = default!;
        public string? From { get; set; } = default!;
    }
}
