using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Infrastructure.Email;
using NewsAggregator.Worker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register your services here
        services.AddScoped<IEmailService, EmailService>();

        // Register the consumer
        services.AddScoped<ArticlePublishedConsumer>();
    })
    .Build();

Console.WriteLine("Starting ArticlePublishedConsumer...");

using var scope = host.Services.CreateScope();
var consumer = scope.ServiceProvider.GetRequiredService<ArticlePublishedConsumer>();
consumer.Start();