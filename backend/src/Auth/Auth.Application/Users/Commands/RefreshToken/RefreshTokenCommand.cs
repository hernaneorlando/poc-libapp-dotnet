using Core.API;

namespace Auth.Application.Users.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a valid refresh token.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

/// <summary>
/// Response model for token refresh operation.
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType = "Bearer");
