using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Infrastructure.Persistence.MongoDb
{
    internal class RefreshTokenDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Token { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public DateTime Created { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Convert from Domain Entity to Mongo Document
        public static RefreshTokenDocument FromDomain(RefreshToken input)
        {
            return new RefreshTokenDocument
            {
                Token = input.Token,
                UserId = input.UserId,
                Expires = input.Expires,
                Created = input.Created,
                IsRevoked = input.IsRevoked,
                IsExpired = input.IsExpired,
                RevokedAt = input.RevokedAt
            };
        }

        // Convert from Mongo Document to Domain Entity
        public RefreshToken ToDomain()
        {
            return new RefreshToken(Id!.ToString(), Token, UserId, Expires, IsRevoked, Created);
        }
    }
}