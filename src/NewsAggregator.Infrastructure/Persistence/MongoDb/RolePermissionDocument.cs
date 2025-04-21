using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NewsAggregator.Infrastructure.Persistence.MongoDb
{
    public class RolePermissionDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? RoleId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? PermissionId { get; set; }

        public RolePermissionDocument()
        {

        }

        public RolePermissionDocument(string? roleId, string? permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
