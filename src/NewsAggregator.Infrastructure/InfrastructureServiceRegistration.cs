using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NewsAggregator.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;

namespace NewsAggregator.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure MongoDB client and database
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? configuration["MongoDbSettings:ConnectionString"];
            var databaseName = Environment.GetEnvironmentVariable("MONGO_DBNAME") ?? configuration["MongoDbSettings:DatabaseName"];

            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            services.AddSingleton(database);
            services.AddScoped<INewsRepository, NewsRepository>();

            var newApiBaseUrl = configuration["NewsApi:BaseUrl"];

            // Register named HttpClient
            services.AddHttpClient(nameof(NewsApiClient), client =>
            {
                client.BaseAddress = new Uri(newApiBaseUrl!);
                client.DefaultRequestHeaders.Add("X-Api-Key", configuration["NewsApi:ApiKey"]);
                client.DefaultRequestHeaders.Add("User-Agent", configuration["NewsApi:UserAgent"]);
            })
            .AddPolicyHandler((services, _) => HttpClientPolicies.GetResiliencePolicy<NewsApiClient>(services.GetRequiredService<ILogger<NewsApiClient>>()));

            // Register the client class for DI
            services.AddScoped<NewsApiClient>();

            return services;
        }
    }
}
