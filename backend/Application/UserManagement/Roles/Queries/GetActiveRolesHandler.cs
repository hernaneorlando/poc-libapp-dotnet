using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Roles.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Roles.Queries;

public class GetActiveRolesHandler(IRoleService roleService) : IRequestHandler<GetActiveRolesQuery, ValidationResult<IEnumerable<RoleDto>>>
{
    private readonly IRoleService roleService = roleService;

    public async Task<ValidationResult<IEnumerable<RoleDto>>> Handle(GetActiveRolesQuery request, CancellationToken cancellationToken)
    {
        var roleResults = await roleService.GetActiveRolesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return roleResults.IsSuccess
            ? ValidationResult.Ok(roleResults.Value.Select(role => (RoleDto)role))
            : ValidationResult.Fail<IEnumerable<RoleDto>>(roleResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve roles.");
    }
}