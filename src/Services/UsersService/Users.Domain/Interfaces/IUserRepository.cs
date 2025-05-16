using Users.Domain.Models;

namespace Users.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> AddAsync(User user);
        bool VerifyUser(string password, string passwordHash);
        Task<(string Token, string refreshToken)> GetToken(string username);
        Task<bool> IsUserExistWhenCreate(string username);
    }
}
