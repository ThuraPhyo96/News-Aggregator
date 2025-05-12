using Users.Application.Common;
using Users.Application.DTOs;

namespace Users.Application.Interfaces
{
    public interface IUserAppService
    {
        Task<Result<UserDto>> GetByUsername(string username);
        Task<Result<UserDto>> CreateUser(CreateUserDto? input);
        Task<Result<RefreshTokenResponseDto>> GetToken(LoginUserDto input);
    }
}
