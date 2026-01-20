namespace Auth.Application.Users.Commands.RefreshToken;

using MediatR;
using Auth.Application.Common;

/// <summary>
/// Command to refresh an access token using a valid refresh token.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;

/// <summary>
/// Response model for token refresh operation.
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType = "Bearer");
