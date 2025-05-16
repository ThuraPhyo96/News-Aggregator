using MongoDB.Driver;
using Users.Domain.Authorization;
using Users.Infrastructure.Persistence.MongoDb;

namespace Users.Infrastructure.Authorization
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly IMongoCollection<PermissionDocument> _permissionCollection;

        public PermissionRepository(IMongoDatabase database)
        {
            _permissionCollection = database.GetCollection<PermissionDocument>("Permissions");
        }

        public async Task<List<string>> GetAllPermissionClaimsAsync()
        {
            return await _permissionCollection.Find(_ => true)
                                              .Project(p => p.ClaimValue!)
                                              .ToListAsync();
        }
    }
}
