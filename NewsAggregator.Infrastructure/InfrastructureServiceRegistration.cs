using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NewsAggregator.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Infrastructure.HttpClients;

namespace NewsAggregator.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure MongoDB client and database
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            services.AddSingleton(database);
            services.AddScoped<INewsRepository, NewsRepository>();

            // Register NewsApiService
            services.AddHttpClient<NewsApiClient>();

            return services;
        }
    }
}
