namespace Auth.Infrastructure.Models;

/// <summary>
/// Relational entity representing a refresh token for persistence.
/// This is separate from the RefreshToken ValueObject in the domain layer.
/// Domain: manages business logic (expiration, revocation checks).
/// Infrastructure: handles persistence lifecycle.
/// </summary>
public sealed class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRememberMe { get; set; }

    /// <summary>
    /// Converts relational entity to domain value object (Entity → Domain).
    /// </summary>
    public static implicit operator RefreshToken(RefreshTokenEntity entity)
    {
        return new RefreshToken
        {
            Token = entity.Token,
            ExpiresAt = entity.ExpiresAt,
            RevokedAt = entity.RevokedAt,
            IsRememberMe = entity.IsRememberMe
        };
    }

    /// <summary>
    /// Converts domain value object to relational entity (Domain → Entity).
    /// </summary>
    public static implicit operator RefreshTokenEntity(RefreshToken token)
    {
        return new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            RevokedAt = token.RevokedAt,
            IsRememberMe = token.IsRememberMe,
            CreatedAt = DateTime.UtcNow
        };
    }
}
