using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;
using FluentResults;

namespace Application.UserManagement.Permissions.Services;

public interface IPermissionService
{
    Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<IEnumerable<PermissionDto>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<Result<Permission>> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task<Result<Permission>> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task<Result> DeletePermissionAsync(Guid id, CancellationToken cancellationToken);
}