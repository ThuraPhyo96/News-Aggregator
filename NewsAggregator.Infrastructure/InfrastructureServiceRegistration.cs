using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NewsAggregator.Infrastructure.Repositories;
using NewsAggregator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

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

            return services;
        }
    }
}
