namespace Auth.Application.Common.Security;

/// <summary>
/// Service for hashing and verifying passwords.
/// Uses bcrypt for secure password hashing.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Hashed password to compare against</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool Verify(string password, string hash);
}
