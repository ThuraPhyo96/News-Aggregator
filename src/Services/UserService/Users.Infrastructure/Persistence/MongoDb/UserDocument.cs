using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Users.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Infrastructure.Helpers;

namespace Users.Infrastructure.Persistence.MongoDb
{
    public class UserDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }

        // Convert from Domain Entity to Mongo Document
        public static UserDocument FromDomain(User user)
        {
            return new UserDocument
            {
                Username = user.Username,
                PasswordHash = PasswordHasher.Hash(user.PasswordHash!)
            };
        }

        // Convert from Mongo Document to Domain Entity
        public User ToDomain()
        {
            return new User(Id!.ToString(), Username, PasswordHash);
        }
    }
}
