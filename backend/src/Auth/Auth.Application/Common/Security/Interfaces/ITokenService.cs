using Auth.Domain.Aggregates.User;

namespace Auth.Application.Common.Security.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the user.
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token for the user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken(Guid userId);

    /// <summary>
    /// Validates and decodes a JWT token.
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Claims if valid, null if invalid</returns>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
