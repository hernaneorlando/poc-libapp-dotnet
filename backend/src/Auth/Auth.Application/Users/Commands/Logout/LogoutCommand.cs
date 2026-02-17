using Core.API;

namespace Auth.Application.Users.Commands.Logout;

/// <summary>
/// Command to logout a user by revoking their refresh token.
/// Represents the Logout operation in the authentication flow.
/// </summary>
public sealed record LogoutCommand(
    long ExternalId,
    string RefreshToken) : IRequest<Result<LogoutResponse>>;

/// <summary>
/// Logout response confirming token revocation.
/// </summary>
public sealed record LogoutResponse(
    bool Success,
    string Message);
