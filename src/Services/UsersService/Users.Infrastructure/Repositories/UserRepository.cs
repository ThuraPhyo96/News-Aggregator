using MongoDB.Driver;
using Users.Domain.Interfaces;
using Users.Domain.Models;
using Users.Infrastructure.Helpers;
using Users.Infrastructure.Persistence.MongoDb;

namespace Users.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserDocument> _userCollection;
        private readonly ITokenRepository _tokenRepository;

        public UserRepository(IMongoDatabase database, ITokenRepository tokenRepository)
        {
            _userCollection = database.GetCollection<UserDocument>("Users");
            _tokenRepository = tokenRepository;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            try
            {
                var user = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
                if (user is null)
                    return null;

                return user.ToDomain();
            }
            catch (Exception)
            {
                throw;
            }
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

        public async Task<(string Token, string refreshToken)> GetToken(string username)
        {
            return await _tokenRepository.GenerateToken(username);
        }

        public async Task<bool> IsUserExistWhenCreate(string username)
        {
            try
            {
                return await _userCollection.Find(u => u.Username == username).AnyAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
