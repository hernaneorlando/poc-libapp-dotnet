namespace Auth.Application.Common.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Auth.Application.Common.Security.Interfaces;
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
                new(ClaimTypes.NameIdentifier, user.ExternalId.ToString()),
                new(ClaimTypes.Email, user.Contact.Email),
                new(ClaimTypes.Name, user.GetFullName()),
                new("username", user.Username.Value),
                new("userType", user.UserType.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique JWT ID
            };

            // Add roles as claims (prepared for PBAC authorization)
            
                var roleClaimValue = user.Roles.Select(role =>
                {
                    return new
                    {
                        role.Name,
                        Permissions = role.Permissions
                            .Where(p => !user.DeniedPermissions.Any(dp => dp.Code == p.Code)) // Exclude denied permissions
                            .Select(p => p.Code).ToList()
                    };    
                });

                var roleJson = JsonSerializer.Serialize(roleClaimValue);
                claims.Add(new Claim(ClaimTypes.Role, roleJson, JsonClaimValueTypes.Json));

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
            _logger.LogError(ex, "Error generating access token for user {UserId}", user.ExternalId);
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
}
