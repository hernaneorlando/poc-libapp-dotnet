namespace Auth.Application.Users.Commands.RemoveDeniedPermission;

using MediatR;
using Auth.Application.Common;

/// <summary>
/// Command to remove a denied permission from a user.
/// Allows a user to perform an action they were explicitly denied.
/// </summary>
public sealed record RemoveDeniedPermissionCommand(
    string UserId,
    string Feature,
    string Action
) : IRequest<Result<RemoveDeniedPermissionResponse>>;

/// <summary>
/// Response model for removing a denied permission.
/// </summary>
public sealed record RemoveDeniedPermissionResponse(
    string UserId,
    string Feature,
    string Action,
    string Message);
