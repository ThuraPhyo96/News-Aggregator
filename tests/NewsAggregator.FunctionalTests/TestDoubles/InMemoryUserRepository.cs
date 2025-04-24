using MongoDB.Bson;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;
using NewsAggregator.Infrastructure.Helpers;
using System.Collections.Concurrent;
using System.Reflection;

namespace NewsAggregator.FunctionalTests.TestDoubles
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<string, User> _users = new();

        public InMemoryUserRepository()
        {
            User adminUser = new(
                "67eeac692d3c4efa816802tt",
                "admin",
                "oaieofaopvejio(00gqjgqejfopipeoifpaoir"
                );

            User user = new(
               "67eeac692d3c4efa816802ff",
               "John",
               "oaieofaopvejio(00gqjgqej"
               );

            _users[adminUser.Username!] = adminUser;
            _users[user.Username!] = user;
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            _users.TryGetValue(username, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> AddAsync(User user)
        {
            var generatedId = ObjectId.GenerateNewId().ToString();

            // Use reflection to set the private Id property
            typeof(User)
                .GetProperty(nameof(Article.Id), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?.SetValue(user, generatedId);

            _users[user.Username!] = user;

            return Task.FromResult<User?>(user)!;
        }

        public bool VerifyUser(string password, string passwordHash)
        {
            return true;
        }

        public async Task<string> GetToken(string username)
        {
            return "0pr4q-03tg0qgoiobhwobwoiuq45ugqu9.kgijtgoi.094";
        }
    }
}
