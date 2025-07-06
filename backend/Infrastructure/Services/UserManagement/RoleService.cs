using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Roles.Services;
using FluentResults;
using Infrastructure.Persistence.Context;
using MongoDB.Driver;

namespace Infrastructure.Services.UserManagement;

public class RoleService(NoSqlDataContext noSqlDataContext) : IRoleService
{
    public async Task<Result<IEnumerable<RoleDto>>> GetActiveRolesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var roles = await noSqlDataContext.Roles
            .Find(x => x.Active)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        if (roles is null || roles.Count == 0)
        {
            return Result.Fail("No active roles found.");
        }

        return Result.Ok(roles.Select(role => (RoleDto)role));
    }
}
