using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NewsAggregator.Infrastructure.Persistence.MongoDb;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsAggregator.Infrastructure.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;
        private readonly IMongoCollection<UserRoleDocument> _userRoleCollection;
        private readonly IMongoCollection<RolePermissionDocument> _rolePermissionCollection;
        private readonly IMongoCollection<PermissionDocument> _permissionCollection;
        private readonly IMongoCollection<UserDocument> _userCollection;

        public JwtTokenGenerator(IConfiguration config, IMongoDatabase database)
        {
            _config = config;
            _userRoleCollection = database.GetCollection<UserRoleDocument>("UserRoles");
            _rolePermissionCollection = database.GetCollection<RolePermissionDocument>("RolePermissions");
            _permissionCollection = database.GetCollection<PermissionDocument>("Permissions");
            _userCollection = database.GetCollection<UserDocument>("Users");
        }

        public async Task<string> GenerateToken(string username)
        {
            string jwtkey = Environment.GetEnvironmentVariable("JWT_Key") ?? _config["Jwt:Key"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var user = await _userCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

            // 1. Get roleIds for user
            var userRoles = await _userRoleCollection
                .Find(ur => ur.UserId == user.Id)
                .ToListAsync();

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // 2. Get permissionIds via RolePermission
            var rolePermissions = await _rolePermissionCollection
                .Find(rp => roleIds.Contains(rp.RoleId))
                .ToListAsync();

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();

            // 3. Get permission values
            var permissions = await _permissionCollection
                .Find(p => permissionIds.Contains(p.Id))
                .ToListAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id!),
                new(ClaimTypes.Name, user.Username!)
            };

            // Add each permission as a separate claim
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission.ClaimValue!));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
