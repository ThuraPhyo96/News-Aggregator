using NewsAggregator.Domain.Models;

namespace NewsAggregator.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> AddAsync(User user);
        bool VerifyUser(string password, string passwordHash);
        string GetToken(string username);
    }
}
