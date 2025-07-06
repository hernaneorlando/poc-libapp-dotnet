using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.UserManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using MongoDB.Driver;

namespace Infrastructure.Services.UserManagement;

public class PermissionService(NoSqlDataContext noSqlDataContext) : IPermissionService
{
    #region Query Methods

    public async Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await noSqlDataContext.Permissions
            .Find(x => x.ExternalId.ToString() == id.ToString())
            .FirstOrDefaultAsync(cancellationToken);

        if (permission is null)
        {
            return Result.Fail($"Permission with ID {id} not found.");
        }

        return Result.Ok((PermissionDto)permission);
    }

    public async Task<Result<IEnumerable<PermissionDto>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var permissions = await noSqlDataContext.Permissions
            .Find(x => x.Active)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        if (permissions is null || permissions.Count == 0)
        {
            return Result.Fail("No active permissions found.");
        }

        return Result.Ok(permissions.Select(permission => (PermissionDto)permission));
    }

    #endregion

    #region Command Methods

    public async Task<Result<Permission>> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        var permissionEntity = new PermissionEntity
        {
            ExternalId = Guid.NewGuid(),
            Code = permission.Code,
            Description = permission.Description,
            Active = true
        };

        await noSqlDataContext.Permissions.InsertOneAsync(permissionEntity, null, cancellationToken);
        return Result.Ok((Permission)permissionEntity);
    }

    public async Task<Result<Permission>> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        var filter = Builders<PermissionEntity>.Filter
            .Where(p => p.Active && p.ExternalId.ToString() == permission.ExternalId.ToString());
        var update = Builders<PermissionEntity>.Update
            .Set(p => p.Description, permission.Description)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);
        var options = new UpdateOptions { IsUpsert = false, };

        var result = await noSqlDataContext.Permissions.UpdateOneAsync(filter, update, options, cancellationToken);
        if (result.ModifiedCount == 0)
        {
            return Result.Fail("Permission not found or no changes made.");
        }

        var updatedPermission = await noSqlDataContext.Permissions
            .Find(p => p.ExternalId.ToString() == permission.ExternalId.ToString())
            .FirstOrDefaultAsync(cancellationToken);
        return Result.Ok((Permission)updatedPermission);
    }

    public async Task<Result> DeletePermissionAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<PermissionEntity>.Filter
            .Where(p => p.Active && p.ExternalId.ToString() == id.ToString());
        var update = Builders<PermissionEntity>.Update
            .Set(p => p.Active, false)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);
        var options = new UpdateOptions { IsUpsert = false, };

        var result = await noSqlDataContext.Permissions.UpdateOneAsync(filter, update, options, cancellationToken);
        if (result.ModifiedCount == 0)
        {
            return Result.Fail("Permission not found or no changes made.");
        }

        return Result.Ok();
    }

    #endregion
}