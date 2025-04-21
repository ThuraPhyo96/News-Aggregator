using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsAggregator.Domain.Models;
using System.Xml.Linq;

namespace NewsAggregator.Infrastructure.Persistence.MongoDb
{
    public class PermissionDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? ClaimValue { get; set; }

        // Convert from Domain Entity to Mongo Document
        public static PermissionDocument FromDomain(Permission input)
        {
            return new PermissionDocument
            {
                DisplayName = input.DisplayName,
                ClaimValue = input.ClaimValue
            };
        }

        // Convert from Mongo Document to Domain Entity
        public Permission ToDomain()
        {
            return new Permission(Id!.ToString(), DisplayName, ClaimValue);
        }
    }
}
