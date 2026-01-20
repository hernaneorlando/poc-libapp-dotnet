namespace Auth.Application.Roles.Commands.CreateRole;

using MediatR;
using Auth.Application.Common;

/// <summary>
/// Command to create a new role with permissions.
/// Supports PBAC (Permission-Based Access Control) pattern.
/// </summary>
public sealed record CreateRoleCommand(
    string Name,
    string Description,
    IReadOnlyList<RolePermissionRequest> Permissions) : IRequest<Result<RoleResponse>>;

/// <summary>
/// Permission request model for role creation.
/// Specifies Feature + Action combination as strings.
/// </summary>
public sealed record RolePermissionRequest(
    string Feature,
    string Action);

/// <summary>
/// Response model for role creation.
/// </summary>
public sealed record RoleResponse(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<RolePermissionResponse> Permissions);

/// <summary>
/// Permission response model in role response.
/// </summary>
public sealed record RolePermissionResponse(
    string Feature,
    string Action);
