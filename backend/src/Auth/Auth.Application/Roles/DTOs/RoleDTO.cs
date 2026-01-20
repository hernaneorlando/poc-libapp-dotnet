namespace Auth.Application.Roles.DTOs;

using Auth.Domain.Aggregates.Role;
using Auth.Domain.Aggregates.Permission;

/// <summary>
/// Data Transfer Object for Role aggregate.
/// </summary>
public sealed record RoleDTO(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<PermissionDTO> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive
)
{
    /// <summary>
    /// Maps a Role aggregate to a RoleDTO.
    /// </summary>
    public static RoleDTO FromDomain(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new RoleDTO(
            Id: role.Id.Value.ToString(),
            Name: role.Name,
            Description: role.Description,
            Permissions: role.Permissions.Select(p => PermissionDTO.FromDomain(p)).ToList(),
            CreatedAt: role.CreatedAt,
            UpdatedAt: role.UpdatedAt,
            IsActive: role.IsActive
        );
    }
}
