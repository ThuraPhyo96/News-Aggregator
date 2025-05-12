using MongoDB.Bson;
using MongoDB.Driver;
using Users.Infrastructure.Persistence.MongoDb;

namespace Users.Infrastructure.Data
{
    public class SeedData
    {
        private readonly IMongoCollection<RoleDocument> _roleCollection;
        private readonly IMongoCollection<PermissionDocument> _permissionCollection;
        private readonly IMongoCollection<RolePermissionDocument> _rolePermissionCollection;
        private readonly IMongoCollection<UserDocument> _userCollection;
        private readonly IMongoCollection<UserRoleDocument> _userRoleCollection;

        public SeedData(IMongoDatabase database)
        {
            _roleCollection = database.GetCollection<RoleDocument>("Roles");
            _permissionCollection = database.GetCollection<PermissionDocument>("Permissions");
            _rolePermissionCollection = database.GetCollection<RolePermissionDocument>("RolePermissions");
            _userRoleCollection = database.GetCollection<UserRoleDocument>("UserRoles");
            _userCollection = database.GetCollection<UserDocument>("Users");
        }

        public async Task SeedAsync()
        {
            //await SeedRolesAsync();
            //await SeedPermissionsAsync();
            //await SeedRolePermissionsAsync();
            //await SeedAdminUserRoleAsync();
        }

        private async Task SeedRolesAsync()
        {
            var existing = await _roleCollection.Find(_ => true).AnyAsync();
            if (existing) return;

            var roles = new List<RoleDocument>
            {
                new() { Id = ObjectId.GenerateNewId().ToString(), Name = "admin", DisplayName = "Administrator" },
                new() { Id = ObjectId.GenerateNewId().ToString(), Name = "user", DisplayName = "User" }
            };

            await _roleCollection.InsertManyAsync(roles);
        }

        private async Task SeedPermissionsAsync()
        {
            var existing = await _permissionCollection.Find(_ => true).AnyAsync();
            if (existing) return;

            var permissions = new List<PermissionDocument>
            {
                new() { Id = ObjectId.GenerateNewId().ToString(), ClaimValue = "articles.read", DisplayName = "Read Articles" },
                new() { Id = ObjectId.GenerateNewId().ToString(), ClaimValue = "articles.write", DisplayName = "Write Articles" }
            };

            await _permissionCollection.InsertManyAsync(permissions);
        }

        private async Task SeedRolePermissionsAsync()
        {
            var rolesDocs = await _roleCollection.Find(_ => true).ToListAsync();
            var permissionsDocs = await _permissionCollection.Find(_ => true).ToListAsync();

            var rolePermissions = new List<RolePermissionDocument>();

            foreach (var role in rolesDocs)
            {
                foreach (var permission in permissionsDocs)
                {
                    var rolePermission = new RolePermissionDocument(role.Id, permission.Id);
                    rolePermissions.Add(rolePermission);
                }
            }

            await _rolePermissionCollection.InsertManyAsync(rolePermissions);
        }

        private async Task SeedAdminUserRoleAsync()
        {
            var adminUser = await _userCollection.Find(x => x.Username == "admin").FirstOrDefaultAsync();
            var adminRole = await _roleCollection.Find(x => x.Name == "admin").FirstOrDefaultAsync();

            var adminUserRole = new UserRoleDocument(adminUser.Id, adminRole.Id);
            await _userRoleCollection.InsertOneAsync(adminUserRole);
        }
    }
}
