using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Roles.DTOs;
using Domain.UserManagement;
using Infrastructure.Common;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class RoleEntity : DocumentDbEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<PermissionEntity> Permissions { get; set; } = [];

    public static implicit operator RoleDto(RoleEntity roleEntity)
    {
        return new RoleDto(roleEntity.Name, roleEntity.Description)
        {
            Permissions = [.. roleEntity.Permissions.Select(x => (PermissionDto)x)]
        };
    }

    public static implicit operator Role(RoleEntity roleEntity)
    {
        var model = new Role(roleEntity.Name, roleEntity.Description)
        {
            Permissions = [.. roleEntity.Permissions.Select(x => (Permission)x)]
        };

        model.ConvertEntityBaseProperties(roleEntity);
        return model;
    }

    public static implicit operator RoleEntity(Role role)
    {
        var entity = new RoleEntity
        {
            Name = role.Name,
            Description = role.Description,
            Permissions = [.. role.Permissions.Select(x => (PermissionEntity)x)]
        };

        entity.ConvertModelBaseProperties(role);
        return entity;
    }
}