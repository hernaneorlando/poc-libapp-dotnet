using Application.Common.BaseDTO;
using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Roles.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Roles.Queries;

public class GetActiveRolesHandler(IRoleService roleService) : IRequestHandler<GetActiveRolesQuery, ValidationResult<PagedResponseDTO<RoleDto>>>
{
    private readonly IRoleService roleService = roleService;

    public async Task<ValidationResult<PagedResponseDTO<RoleDto>>> Handle(GetActiveRolesQuery request, CancellationToken cancellationToken)
    {
        var roleResults = await roleService.GetActiveRolesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return roleResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<RoleDto> { Data = [..roleResults.Value] })
            : ValidationResult.Fail<PagedResponseDTO<RoleDto>>(roleResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve roles.");
    }
}