using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Users.Domain.Interfaces;
using Users.Domain.Models;
using Users.Infrastructure.Persistence.MongoDb;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Users.Infrastructure.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _config;
        private readonly IMongoCollection<UserRoleDocument> _userRoleCollection;
        private readonly IMongoCollection<RolePermissionDocument> _rolePermissionCollection;
        private readonly IMongoCollection<PermissionDocument> _permissionCollection;
        private readonly IMongoCollection<UserDocument> _userCollection;
        private readonly IMongoCollection<RefreshTokenDocument> _refreshTokenCollection;

        public TokenRepository(IConfiguration config, IMongoDatabase database)
        {
            _config = config;
            _userRoleCollection = database.GetCollection<UserRoleDocument>("UserRoles");
            _rolePermissionCollection = database.GetCollection<RolePermissionDocument>("RolePermissions");
            _permissionCollection = database.GetCollection<PermissionDocument>("Permissions");
            _userCollection = database.GetCollection<UserDocument>("Users");
            _refreshTokenCollection = database.GetCollection<RefreshTokenDocument>("RefreshTokens");
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateToken(string username)
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

            // Generate secure refresh token
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenObj = new RefreshToken(refreshToken, user.Id, DateTime.Now.AddDays(7), false, DateTime.Now);

            await AddRefreshToken(refreshTokenObj);

            var secureToken = new JwtSecurityTokenHandler().WriteToken(token);
            return (secureToken, refreshToken);
        }

        public async Task<RefreshToken?> GetByTokenAndUserIdAsync(string refreshToken, string userId)
        {
            try
            {
                var doc = await _refreshTokenCollection.Find(u => u.Token == refreshToken && u.UserId == userId).FirstOrDefaultAsync();
                if (doc is null)
                    return null;

                return doc.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RefreshToken> AddRefreshToken(RefreshToken input)
        {
            try
            {
                var doc = RefreshTokenDocument.FromDomain(input);
                await _refreshTokenCollection.InsertOneAsync(doc);
                return doc.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<long> UpdateRefreshToken(string id, RefreshToken input)
        {
            try
            {
                var existingObj = await _refreshTokenCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
                if (existingObj is null)
                    return 0;

                var updateDoc = RefreshTokenDocument.FromDomain(input);
                updateDoc.Id = id;

                var result = await _refreshTokenCollection.ReplaceOneAsync(
                    doc => doc.Id == id,
                    updateDoc
                );

                return result.ModifiedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
