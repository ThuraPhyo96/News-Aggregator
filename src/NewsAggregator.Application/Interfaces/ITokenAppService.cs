using NewsAggregator.Application.Common;
using NewsAggregator.Application.DTOs;

namespace NewsAggregator.Application.Interfaces
{
    public interface ITokenAppService
    {
        Task<Result<RefreshTokenResponseDto>> RefreshToken(RefreshTokenRequestProcessDto input);
    }
}
