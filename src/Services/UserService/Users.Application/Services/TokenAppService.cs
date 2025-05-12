using Users.Application.Common;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Domain.Interfaces;

namespace Users.Application.Services
{
    public class TokenAppService : ITokenAppService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;

        public TokenAppService(ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<RefreshTokenResponseDto>> RefreshToken(RefreshTokenRequestProcessDto input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.UserId))
                    return Result<RefreshTokenResponseDto>.Fail("User can not be null or empty.");

                if (string.IsNullOrEmpty(input.RefreshToken))
                    return Result<RefreshTokenResponseDto>.Fail("Refresh token can not be null or empty.");

                var user = await _userRepository.GetByIdAsync(input.UserId);
                if (user is null)
                    return Result<RefreshTokenResponseDto>.Fail("Invalid user.");

                var existingRefreshToken = await _tokenRepository.GetByTokenAndUserIdAsync(input.RefreshToken, user?.Id!);

                if (existingRefreshToken == null || existingRefreshToken.IsRevoked || existingRefreshToken.IsExpired)
                {
                    return Result<RefreshTokenResponseDto>.Fail("Invalid or expired refresh token.");
                }

                var updateRefreshToken = new UpdateRefreshTokenDto()
                {
                    Token = existingRefreshToken.Token,
                    UserId = existingRefreshToken.UserId,
                    Created = existingRefreshToken.Created,
                    IsRevoked = true,
                    RevokedAt = DateTime.Now,
                    Expires = existingRefreshToken.Expires,
                    IsExpired = existingRefreshToken.IsExpired
                };

                var updateRefreshTokenObj = TokenMapper.ToEntity(updateRefreshToken);
                if (updateRefreshTokenObj is null)
                    return Result<RefreshTokenResponseDto>.Fail("Invalid refresh token.");

                var updatedCount = await _tokenRepository.UpdateRefreshToken(existingRefreshToken.Id!, updateRefreshTokenObj);
                if (updatedCount == 0)
                    return Result<RefreshTokenResponseDto>.Fail("Failed to update the refresh token.");

                // Generate new tokens
                var (newAccessToken, newRefreshToken) = await _tokenRepository.GenerateToken(user?.Username!);

                var newRefreshTokenDto = new RefreshTokenResponseDto()
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
                return Result<RefreshTokenResponseDto>.Ok(newRefreshTokenDto);
            }
            catch (Exception ex)
            {
                return Result<RefreshTokenResponseDto>.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}
