using Users.Application.Common;
using Users.Application.DTOs;
using Users.Application.Helpers;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Domain.Interfaces;

namespace Users.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly IUserRepository _userRepository;

        public UserAppService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDto>> GetByUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return Result<UserDto>.Fail("User name can not be null or empty.");

                if (!UsernameValidationHelper.IsValidUsername(username))
                    return Result<UserDto>.Fail("User name contains invalid characters.");

                var obj = await _userRepository.GetByUsernameAsync(username.ToLowerInvariant());
                if (obj is null)
                    return Result<UserDto>.Fail("Not found!");

                return Result<UserDto>.Ok(UserMapper.ToDto(obj!)!);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<UserDto>> CreateUser(CreateUserDto? input)
        {
            try
            {
                if (input is null)
                    return Result<UserDto>.Fail("User can not be null.");

                if (string.IsNullOrWhiteSpace(input.Username) ||
                    string.IsNullOrWhiteSpace(input.Password))
                {
                    return Result<UserDto>.Fail("Username and Password cannot be empty or whitespace.");
                }

                if (!UsernameValidationHelper.IsValidUsername(input.Username))
                    return Result<UserDto>.Fail("User name contains invalid characters.");

                if (await _userRepository.IsUserExistWhenCreate(input.Username))
                    return Result<UserDto>.Fail("User name already existed.");

                var user = UserMapper.ToEntity(input);
                if (user is null)
                    return Result<UserDto>.Fail("Invalid user data.");

                var returnUser = await _userRepository.AddAsync(user);
                if (returnUser is null)
                    return Result<UserDto>.Fail("Failed to save user.");

                var dto = UserMapper.ToDto(returnUser);
                return Result<UserDto>.Ok(dto!);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<RefreshTokenResponseDto>> GetToken(LoginUserDto input)
        {
            try
            {
                if (input is null)
                    return Result<RefreshTokenResponseDto>.Fail("Login user can not be null.");

                if (string.IsNullOrWhiteSpace(input.Username) ||
                   string.IsNullOrWhiteSpace(input.Password))
                {
                    return Result<RefreshTokenResponseDto>.Fail("Username and Password cannot be empty or whitespace.");
                }

                var user = await _userRepository.GetByUsernameAsync(input.Username.ToLowerInvariant());
                if (user == null)
                    return Result<RefreshTokenResponseDto>.Fail($"Invalid credentials.");

                bool isVerify = _userRepository.VerifyUser(input.Password!, user!.PasswordHash!);
                if (!isVerify)
                    return Result<RefreshTokenResponseDto>.Fail($"Invalid credentials.");

                var secureToken = await _userRepository.GetToken(user.Username!);
                var refreshTokenDto = new RefreshTokenResponseDto()
                {
                    AccessToken = secureToken.Token,
                    RefreshToken = secureToken.refreshToken
                };
                return Result<RefreshTokenResponseDto>.Ok(refreshTokenDto);
            }
            catch (Exception ex)
            {
                return Result<RefreshTokenResponseDto>.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}
