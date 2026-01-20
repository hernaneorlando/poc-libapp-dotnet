namespace Auth.Application.Users.Commands.AddDeniedPermission;

using Auth.Domain.Enums;
using MediatR;
using Auth.Application.Common;

/// <summary>
/// Command to add a denied permission to a user.
/// Allows administrators to create exceptions where a user cannot perform a specific action
/// even if their roles grant that permission.
/// </summary>
public sealed record AddDeniedPermissionCommand(
    string UserId,
    string Feature,
    string Action
) : IRequest<Result<AddDeniedPermissionResponse>>;

/// <summary>
/// Response model for adding a denied permission.
/// </summary>
public sealed record AddDeniedPermissionResponse(
    string UserId,
    string Feature,
    string Action,
    string Message);
