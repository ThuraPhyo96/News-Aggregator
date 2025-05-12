using Users.Domain.Models;

namespace Users.Domain.Interfaces
{
    public interface ITokenRepository
    {
        Task<(string AccessToken, string RefreshToken)> GenerateToken(string username);
        Task<RefreshToken?> GetByTokenAndUserIdAsync(string refreshToken, string userId);
        Task<RefreshToken> AddRefreshToken(RefreshToken input);
        Task<long> UpdateRefreshToken(string id, RefreshToken input);
    }
}
