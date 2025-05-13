using Notification.Application.Interfaces;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Email;


Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

Host.CreateDefaultBuilder(args)
     .ConfigureAppConfiguration((hostingContext, config) =>
     {
         config.AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
     })
    .ConfigureServices((context, services) =>
    {
        // Register your services here
        services.AddScoped<IEmailService, EmailService>();

        // Register background service
        services.AddHostedService<ArticleConsumerService>();
    })
    .Build()
    .Run();