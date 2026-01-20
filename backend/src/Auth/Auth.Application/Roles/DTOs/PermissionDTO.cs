namespace Auth.Application.Roles.DTOs;

using Auth.Domain.Aggregates.Permission;

/// <summary>
/// Data Transfer Object for Permission ValueObject.
/// </summary>
public sealed record PermissionDTO(
    string Feature,
    string Action,
    string Code
)
{
    /// <summary>
    /// Maps a Permission ValueObject to a PermissionDTO.
    /// </summary>
    public static PermissionDTO FromDomain(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionDTO(
            Feature: permission.Feature.ToString(),
            Action: permission.Action.ToString(),
            Code: permission.Code
        );
    }
}
