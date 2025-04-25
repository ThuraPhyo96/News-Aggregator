using NewsAggregator.Domain.Models;

namespace NewsAggregator.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> AddAsync(User user);
        bool VerifyUser(string password, string passwordHash);
        Task<string> GetToken(string username);
        Task<bool> IsUserExistWhenCreate(string username);
    }
}
