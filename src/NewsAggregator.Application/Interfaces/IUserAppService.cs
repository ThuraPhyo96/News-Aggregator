using NewsAggregator.Application.Common;
using NewsAggregator.Application.DTOs;

namespace NewsAggregator.Application.Interfaces
{
    public interface IUserAppService
    {
        Task<Result<UserDto>> GetByUsername(string username);
        Task<Result<UserDto>> CreateUser(CreateUserDto? input);
        Task<Result<RefreshTokenResponseDto>> GetToken(LoginUserDto input);
    }
}
