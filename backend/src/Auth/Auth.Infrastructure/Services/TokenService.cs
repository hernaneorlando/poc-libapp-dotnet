namespace Auth.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Common.Security;
using Auth.Domain.Aggregates.User;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// JWT token generation and validation service.
/// Implements ITokenService interface.
/// </summary>
public sealed class TokenService(JwtSettings _jwtSettings, ILogger<TokenService> _logger) : ITokenService
{
    /// <summary>
    /// Generates a JWT access token with user claims.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            // Build claims with user info and roles (for PBAC support)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
                new(ClaimTypes.Email, user.Contact.Email),
                new(ClaimTypes.Name, user.GetFullName()),
                new("username", user.Username.Value)
            };

            // Add roles as claims (prepared for PBAC authorization)
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // Add permissions as claims (for PBAC authorization in next phase)
            // This will be populated when permission evaluation is implemented
            var permissions = GetUserPermissions(user);
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Generates a refresh token (random string for token rotation).
    /// </summary>
    public string GenerateRefreshToken(Guid userId)
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a JWT token and returns claims if valid.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Helper method to extract permissions from user and their roles.
    /// Prepared for PBAC implementation - will be extended in authorization phase.
    /// </summary>
    private static List<string> GetUserPermissions(User user)
    {
        var permissions = new List<string>();

        // Get permissions from all user roles
        foreach (var role in user.Roles)
        {
            foreach (var permission in role.Permissions)
            {
                var permissionString = $"{permission.Feature}:{permission.Action}";
                if (!permissions.Contains(permissionString))
                {
                    permissions.Add(permissionString);
                }
            }
        }

        // Remove any explicitly denied permissions
        foreach (var deniedPermission in user.DeniedPermissions)
        {
            var permissionString = $"{deniedPermission.Feature}:{deniedPermission.Action}";
            permissions.Remove(permissionString);
        }

        return permissions;
    }
}
