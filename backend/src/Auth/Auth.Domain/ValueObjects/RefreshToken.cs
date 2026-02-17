namespace Auth.Domain.ValueObjects;

/// <summary>
/// Value Object representing a refresh token.
/// Used for JWT refresh token management and validation.
/// </summary>
public sealed class RefreshToken : ValueObject
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsRememberMe { get; set; }

    public RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <param name="expiresAt">When the token expires.</param>
    /// <param name="isRememberMe">Whether this is a long-lived RememberMe token (true) or sliding-window token (false).</param>
    public static RefreshToken Create(string token, DateTime expiresAt, bool isRememberMe = false)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty");

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Token expiration must be in the future");

        return new RefreshToken
        {
            Token = token,
            ExpiresAt = expiresAt,
            IsRememberMe = isRememberMe
        };
    }

    /// <summary>
    /// Checks if the refresh token is still valid.
    /// </summary>
    public bool IsValid => !IsExpired && !IsRevoked;

    /// <summary>
    /// Checks if the refresh token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if the refresh token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token is already revoked");

        RevokedAt = DateTime.UtcNow;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Token;
    }
}
