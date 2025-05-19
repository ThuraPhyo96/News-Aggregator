using Notification.Application.Interfaces;
using Notification.Infrastructure.Email;
using Notification.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add configuration files
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// Register services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<ArticleConsumerService>();

// Dummy web server to bind port (needed for Render free tier)
var app = builder.Build();
app.MapGet("/", () => "Notification API (Background Worker) is running.");
app.Run();
