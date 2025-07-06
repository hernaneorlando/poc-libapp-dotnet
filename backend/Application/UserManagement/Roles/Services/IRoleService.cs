using Application.UserManagement.Roles.DTOs;
using FluentResults;

namespace Application.UserManagement.Roles.Services;

public interface IRoleService
{
    Task<Result<IEnumerable<RoleDto>>> GetActiveRolesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
