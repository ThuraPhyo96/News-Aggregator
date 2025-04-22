using DnsClient;
using MongoDB.Driver;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;
using NewsAggregator.Infrastructure.Helpers;
using NewsAggregator.Infrastructure.Persistence.MongoDb;

namespace NewsAggregator.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserDocument> _userCollection;
        private readonly JwtTokenGenerator _tokenGenerator;

        public UserRepository(IMongoDatabase database, JwtTokenGenerator tokenGenerator)
        {
            _userCollection = database.GetCollection<UserDocument>("Users");
            _tokenGenerator = tokenGenerator;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                var user = await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
                if (user is null)
                    return null;

                return user.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User?> AddAsync(User user)
        {
            try
            {
                var userDoc = UserDocument.FromDomain(user);
                await _userCollection.InsertOneAsync(userDoc);
                return userDoc.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool VerifyUser(string password, string passwordHash)
        {
            return PasswordHasher.Verify(password, passwordHash);
        }

        public async Task<string> GetToken(string username)
        {
            return await _tokenGenerator.GenerateToken(username);
        }
    }
}
