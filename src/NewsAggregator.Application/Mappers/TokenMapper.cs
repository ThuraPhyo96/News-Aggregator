using NewsAggregator.Application.DTOs;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Mappers
{
    public class TokenMapper
    {
        public static RefreshTokenDto? ToDto(RefreshToken input)
        {
            if (input == null) return null;

            return new RefreshTokenDto
            {
                Id = input.Id,
                Token = input.Token,
                UserId = input.UserId,
                Expires = input.Expires,
                IsExpired = input.IsExpired,
                IsRevoked = input.IsRevoked,
                RevokedAt = input.RevokedAt,
                Created = input.Created
            };
        }

        public static RefreshToken? ToEntity(UpdateRefreshTokenDto input)
        {
            if (input == null) return null;

            return new RefreshToken(
                input.Token,
                input.UserId,
                input.Expires,
                input.IsExpired,
                input.IsRevoked,
                input.Created,
                input.RevokedAt);
        }
    }
}
