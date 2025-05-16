using Users.Application.Common;
using Users.Application.DTOs;

namespace Users.Application.Interfaces
{
    public interface ITokenAppService
    {
        Task<Result<RefreshTokenResponseDto>> RefreshToken(RefreshTokenRequestProcessDto input);
    }
}
