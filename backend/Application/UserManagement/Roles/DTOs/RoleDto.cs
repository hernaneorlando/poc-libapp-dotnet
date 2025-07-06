using Application.Common;
using Application.SeedWork.BaseDTO;
using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;

namespace Application.UserManagement.Roles.DTOs;

public record RoleDto(string Name, string Description) : BaseDto
{
    public List<PermissionDto> Permissions { get; set; } = [];

    public static implicit operator RoleDto(Role role)
    {
        var roleDto = new RoleDto(role.Name, role.Description)
        {
            Permissions = [.. role.Permissions.Select(p => (PermissionDto)p)]
        };

        roleDto.ConvertModelBaseProperties(role);
        return roleDto;
    }
}