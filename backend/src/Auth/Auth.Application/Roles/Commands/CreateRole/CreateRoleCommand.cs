using Auth.Application.Roles.DTOs;
using Core.API;

namespace Auth.Application.Roles.Commands.CreateRole;

/// <summary>
/// Command to create a new role with permissions.
/// Supports PBAC (Permission-Based Access Control) pattern.
/// </summary>
public sealed record CreateRoleCommand(
    string Name,
    string Description,
    IReadOnlyList<RolePermissionRequest> Permissions) : IRequest<Result<RoleDTO>>;

/// <summary>
/// Permission request model for role creation.
/// Specifies Feature + Action combination as strings.
/// </summary>
public sealed record RolePermissionRequest(
    string Feature,
    string Action);