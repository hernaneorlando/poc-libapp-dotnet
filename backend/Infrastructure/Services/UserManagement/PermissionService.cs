using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using Domain.UserManagement;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using MongoDB.Driver;

namespace Infrastructure.Services.UserManagement;

public class PermissionService(NoSqlDataContext noSqlDataContext) : IPermissionService
{
    #region Query Methods

    public async Task<ValidationResult<PermissionDto>> GetPermissionDtoByIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var permissionEntity = await GetByExternalId(externalId, cancellationToken);
        return permissionEntity is not null
            ? ValidationResult.Ok((PermissionDto)permissionEntity)
            : ValidationResult.Fail<PermissionDto>($"Permission with ID {externalId} not found.");
    }

    public async Task<ValidationResult<Permission>> GetPermissionByIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var permissionEntity = await GetByExternalId(externalId, cancellationToken);
        return permissionEntity is not null
            ? ValidationResult.Ok((Permission)permissionEntity)
            : ValidationResult.Fail<Permission>($"Permission with ID {externalId} not found.");
    }

    private async Task<PermissionEntity?> GetByExternalId(Guid externalId, CancellationToken cancellationToken)
    {
        return await noSqlDataContext.Permissions
            .Find(x => x.Active && x.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValidationResult<IEnumerable<PermissionDto>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var permissions = await noSqlDataContext.Permissions
            .Find(x => x.Active)
            .SortBy(x => x.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return permissions.Count != 0
            ? ValidationResult.Ok(permissions.Select(permission => (PermissionDto)permission))
            : ValidationResult.Fail<IEnumerable<PermissionDto>>("No active permissions found.");
    }

    #endregion

    #region Command Methods

    public async Task<ValidationResult<Permission>> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        var permissionEntity = new PermissionEntity
        {
            ExternalId = Guid.NewGuid(),
            Code = permission.Code,
            Description = permission.Description,
            Active = true
        };

        await noSqlDataContext.Permissions.InsertOneAsync(permissionEntity, null, cancellationToken);
        return ValidationResult.Ok((Permission)permissionEntity);
    }

    public async Task<ValidationResult<Permission>> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        var filter = Builders<PermissionEntity>.Filter
            .Where(p => p.Active && p.ExternalId == permission.ExternalId);
        var update = Builders<PermissionEntity>.Update
            .Set(p => p.Description, permission.Description)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);
        var options = new UpdateOptions { IsUpsert = false, };

        var result = await noSqlDataContext.Permissions.UpdateOneAsync(filter, update, options, cancellationToken);
        if (result.ModifiedCount == 0)
        {
            return ValidationResult.Fail<Permission>("Permission not found or no changes made.");
        }

        var updatedPermission = await noSqlDataContext.Permissions
            .Find(p => p.ExternalId == permission.ExternalId)
            .FirstOrDefaultAsync(cancellationToken);
        return ValidationResult.Ok((Permission)updatedPermission);
    }

    #endregion
}