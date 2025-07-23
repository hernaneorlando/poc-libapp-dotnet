using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetPermissionByIdHandler(IPermissionService permissionService) : IRequestHandler<GetPermissionByIdQuery, ValidationResult<PermissionDto>>
{
    public async Task<ValidationResult<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permissionResult = await permissionService.GetPermissionByIdAsync(Guid.Parse(request.Id), cancellationToken);
        return permissionResult.IsSuccess
            ? ValidationResult.Ok(permissionResult.Value)
            : ValidationResult.Fail<PermissionDto>(permissionResult.Errors.FirstOrDefault()?.Message ?? $"Permission with ID {request.Id} not found.");
    }
}