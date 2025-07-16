using Application.Common.BaseDTO;
using Domain.UserManagement;

namespace Application.UserManagement.Permissions.DTOs;

public record PermissionDto(string Code, string Description) : BaseDto
{
    public static implicit operator PermissionDto(Permission permission)
    {
        var permissionDto = new PermissionDto(permission.Code, permission.Description);
        permissionDto.ConvertBaseProperties(permission);
        return permissionDto;
    }
}
