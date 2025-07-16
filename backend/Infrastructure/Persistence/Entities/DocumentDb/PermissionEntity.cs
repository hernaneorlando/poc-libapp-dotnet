using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;
using Infrastructure.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class PermissionEntity : DocumentDbEntity
{
    public required string Code { get; set; }
    public required string Description { get; set; }

    public static implicit operator PermissionDto(PermissionEntity permissionEntity)
    {
        return new PermissionDto(permissionEntity.Code, permissionEntity.Description)
        {
            Id = permissionEntity.ExternalId,
            Active = permissionEntity.Active,
            CreatedAt = permissionEntity.CreatedAt,
            UpdatedAt = permissionEntity.UpdatedAt,
        };
    }

    public static implicit operator Permission(PermissionEntity permissionEntity)
    {
        var model = new Permission
        {
            Code = permissionEntity.Code,
            Description = permissionEntity.Description
        };

        model.ConvertEntityBaseProperties(permissionEntity);
        return model;
    }

    public static implicit operator PermissionEntity(Permission permission)
    {
        var entity = new PermissionEntity
        {
            Code = permission.Code,
            Description = permission.Description,
        };
        
        entity.ConvertModelBaseProperties(permission);
        return entity;
    }
}