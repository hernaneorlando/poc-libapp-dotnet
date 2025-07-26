using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetActivePermissionsHandler(IPermissionService permissionService) : IRequestHandler<GetActivePermissionsQuery, ValidationResult<IEnumerable<PermissionDto>>>
{
    public async Task<ValidationResult<IEnumerable<PermissionDto>>> Handle(GetActivePermissionsQuery request, CancellationToken cancellationToken)
    {
        return await permissionService.GetActivePermissionsAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}