using Domain.UserManagement;
using FluentResults;

namespace Application.UserManagement.Roles.Services;

public interface IRoleService
{
    Task<Result<IEnumerable<Role>>> GetActiveRolesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
