using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Roles.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Roles.Queries;

public class GetActiveRolesHandler(IRoleService roleService) : IRequestHandler<GetActiveRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IRoleService roleService = roleService;

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetActiveRolesQuery request, CancellationToken cancellationToken)
    {
        var roleResults = await roleService.GetActiveRolesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return roleResults.IsSuccess
            ? Result.Ok(roleResults.Value.Select(role => (RoleDto)role))
            : Result.Fail<IEnumerable<RoleDto>>(roleResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve roles.");
    }
}