using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NewsAggregator.Infrastructure.Persistence.MongoDb
{
    public class UserRoleDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? RoleId { get; set; }

        public UserRoleDocument()
        {

        }

        public UserRoleDocument(string? userId, string? roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
