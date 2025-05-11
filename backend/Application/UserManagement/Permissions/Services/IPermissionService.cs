using Domain.UserManagement;
using FluentResults;

namespace Application.UserManagement.Permissions.Services;

public interface IPermissionService
{
    Task<Result<IEnumerable<Permission>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Result<Permission>> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<Permission>> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken);
}
