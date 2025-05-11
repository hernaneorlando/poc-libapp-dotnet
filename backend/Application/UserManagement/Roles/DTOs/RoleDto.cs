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

    public static implicit operator Role(RoleDto roleDto)
    {
        var role = new Role(roleDto.Name, roleDto.Description)
        {
            Permissions = [.. roleDto.Permissions.Select(p => (Permission)p)]
        };

        role.ConvertDtoBaseProperties(roleDto);
        return role;
    }
}