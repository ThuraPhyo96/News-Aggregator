using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Users.Domain.Models;

namespace Users.Infrastructure.Persistence.MongoDb
{
    public class RoleDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }

        // Convert from Domain Entity to Mongo Document
        public static RoleDocument FromDomain(Role input)
        {
            return new RoleDocument
            {
                Name = input.Name,
                DisplayName = input.DisplayName
            };
        }

        // Convert from Mongo Document to Domain Entity
        public Role ToDomain()
        {
            return new Role(Id!.ToString(), Name, DisplayName);
        }
    }
}
