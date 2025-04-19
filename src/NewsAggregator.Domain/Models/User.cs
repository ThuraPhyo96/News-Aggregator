using System;

namespace NewsAggregator.Domain.Models
{
    public class User
    {
        public string? Id { get; private set; }
        public string? Username { get; private set; }
        public string? PasswordHash { get; private set; }

        public User()
        {

        }

        public User(string? username, string? passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        public User(string? id, string? username, string? passwordHash)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
        }
    }
}
