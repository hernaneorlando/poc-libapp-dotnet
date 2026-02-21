using Auth.Domain.Enums;
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
/// Login response with JWT tokens and user info.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserLoginInfo User);

/// <summary>
/// User information in login response.
/// </summary>
public sealed record UserLoginInfo(
    long ExternalId,
    string Username,
    string Email,
    string FullName,
    UserType UserType,
    List<RoleInfo> Roles);

public sealed record RoleInfo(
    string Name,
    List<string> Permissions);