namespace NewsAggregator.Domain.Models
{
    public class RefreshToken
    {
        public string? Id { get; private set; }
        public string? Token { get; private set; }
        public string? UserId { get; private set; }

        public DateTime Expires { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsExpired { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public RefreshToken()
        {
            IsExpired = DateTime.Now >= Expires;
        }

        public RefreshToken(string? token, string? userId, DateTime expires, bool isRevoked, DateTime created)
        {
            Token = token;
            UserId = userId;
            Expires = expires;
            IsRevoked = isRevoked;
            Created = created;
        }

        public RefreshToken(string? token, string? userId, DateTime expires, bool isExpired, bool isRevoked, DateTime created, DateTime? revokedAt)
        {
            Token = token;
            UserId = userId;
            Expires = expires;
            IsExpired = isExpired;
            Created = created;
            IsRevoked = isRevoked;
            RevokedAt = revokedAt;
        }

        public RefreshToken(string? id, string? token, string? userId, DateTime expires, bool isRevoked, DateTime created)
        {
            Id = id;
            Token = token;
            UserId = userId;
            Expires = expires;
            IsRevoked = isRevoked;
            Created = created;
        }
    }
}
