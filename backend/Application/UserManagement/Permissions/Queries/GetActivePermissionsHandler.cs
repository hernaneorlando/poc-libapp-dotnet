using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetActivePermissionsHandler(IPermissionService permissionService) : IRequestHandler<GetActivePermissionsQuery, ValidationResult<IEnumerable<PermissionDto>>>
{
    public async Task<ValidationResult<IEnumerable<PermissionDto>>> Handle(GetActivePermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissionResults = await permissionService.GetActivePermissionsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return permissionResults.IsSuccess
            ? ValidationResult.Ok(permissionResults.Value.Select(permission => permission))
            : ValidationResult.Fail<IEnumerable<PermissionDto>>(permissionResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve permissions.");
    }
}