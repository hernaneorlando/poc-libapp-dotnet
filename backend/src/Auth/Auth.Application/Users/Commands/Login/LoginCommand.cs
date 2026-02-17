using Core.API;

namespace Auth.Application.Users.Commands.Login;

/// <summary>
/// Command to authenticate a user and obtain JWT tokens.
/// Represents the Login operation in the authentication flow.
/// </summary>
public sealed record LoginCommand(
    string Username,
    string Password,
    bool RememberMe = false) : IRequest<Result<LoginResponse>>;

/// <summary>
/// User information in login response.
/// </summary>
public sealed record UserLoginInfo(
    long ExternalId,
    string Username,
    string Email,
    string FullName,
    List<string> Roles);

/// <summary>
/// Login response with JWT tokens and expiration info.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserLoginInfo User);
