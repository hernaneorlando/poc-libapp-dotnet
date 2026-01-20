namespace Auth.Application.Users.DTOs;

using Auth.Domain.Aggregates.User;
using Auth.Application.Roles.DTOs;

/// <summary>
/// Data Transfer Object for User aggregate.
/// </summary>
public sealed record UserDTO(
    string Id,
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string? PhoneNumber,
    IReadOnlyList<RoleDTO> Roles,
    IReadOnlyList<PermissionDTO> DeniedPermissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive
)
{
    /// <summary>
    /// Maps a User aggregate to a UserDTO.
    /// </summary>
    public static UserDTO FromDomain(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDTO(
            Id: user.Id.Value.ToString(),
            FirstName: user.FirstName,
            LastName: user.LastName,
            Username: user.Username.Value,
            Email: user.Contact.Email,
            PhoneNumber: user.Contact.PhoneNumber,
            Roles: user.Roles.Select(r => RoleDTO.FromDomain(r)).ToList(),
            DeniedPermissions: user.DeniedPermissions.Select(p => PermissionDTO.FromDomain(p)).ToList(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            IsActive: user.IsActive
        );
    }
}
