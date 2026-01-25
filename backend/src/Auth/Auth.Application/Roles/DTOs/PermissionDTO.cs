namespace Auth.Application.Roles.DTOs;

using Auth.Domain.Aggregates.Permission;

/// <summary>
/// Data Transfer Object for Permission ValueObject.
/// </summary>
public sealed record PermissionDTO(string Code)
{
    /// <summary>
    /// Maps a Permission ValueObject to a PermissionDTO.
    /// </summary>
    public static implicit operator PermissionDTO(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionDTO(Code: permission.Code);
    }
}
