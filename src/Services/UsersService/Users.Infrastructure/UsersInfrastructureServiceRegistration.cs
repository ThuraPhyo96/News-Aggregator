using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Users.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Users.Domain.Interfaces;
using Users.Infrastructure.Data;
using Users.Infrastructure.Authorization;
using Users.Domain.Authorization;

namespace Users.Infrastructure
{
    public static class UsersInfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure MongoDB client and database
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? configuration["MongoDbSettings:ConnectionString"];
            var databaseName = Environment.GetEnvironmentVariable("MONGO_DBNAME") ?? configuration["MongoDbSettings:DatabaseName"];

            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            services.AddSingleton(database);
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            // Register the client class for DI
            services.AddSingleton<SeedData>();

            return services;
        }
    }
}
