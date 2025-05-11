using Application.UserManagement.Permissions.Services;
using Domain.UserManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PermissionService(NoSqlDataContext noSqlDataContext) : IPermissionService
{
    public async Task<Result<IEnumerable<Permission>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
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

        return Result.Ok(permissions.Select(permission => (Permission)permission));
    }

    public async Task<Result<Permission>> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await noSqlDataContext.Permissions
            .Find(x => x.ExternalId.ToString() == id.ToString())
            .FirstOrDefaultAsync(cancellationToken);

        if (permission is null)
        {
            return Result.Fail($"Permission with ID {id} not found.");
        }

        return Result.Ok((Permission)permission);
    }

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
}
