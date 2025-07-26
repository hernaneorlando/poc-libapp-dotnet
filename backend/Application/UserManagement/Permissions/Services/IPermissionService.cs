using Application.UserManagement.Permissions.DTOs;
using Domain.Common;
using Domain.UserManagement;

namespace Application.UserManagement.Permissions.Services;

public interface IPermissionService
{
    Task<ValidationResult<IEnumerable<PermissionDto>>> GetActivePermissionsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<ValidationResult<PermissionDto>> GetPermissionDtoByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ValidationResult<Permission>> GetPermissionByIdAsync(Guid guid, CancellationToken cancellationToken);

    Task<ValidationResult<Permission>> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task<ValidationResult<Permission>> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken);
}