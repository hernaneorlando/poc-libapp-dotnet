namespace Auth.Infrastructure.Services;

using Auth.Application.Common.Security;
using BCrypt.Net;

/// <summary>
/// Password hashing service using bcrypt.
/// Provides secure password hashing and verification.
/// </summary>
public sealed class PasswordHasher(ILogger<PasswordHasher> _logger) : IPasswordHasher
{
    private const int WorkFactor = 11; // bcrypt work factor (between 4-31)

    /// <summary>
    /// Hashes a password using bcrypt with configurable work factor.
    /// </summary>
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        try
        {
            return BCrypt.HashPassword(password, workFactor: WorkFactor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    /// <summary>
    /// Verifies a password against its bcrypt hash.
    /// </summary>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
    }
}
