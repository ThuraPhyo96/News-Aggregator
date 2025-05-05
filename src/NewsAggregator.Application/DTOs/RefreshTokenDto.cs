namespace NewsAggregator.Application.DTOs
{
    public class RefreshTokenDto
    {
        public string? Id { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }

        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public DateTime Created { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

    public class UpdateRefreshTokenDto
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }

        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public DateTime Created { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class RefreshTokenRequestProcessDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserId { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
