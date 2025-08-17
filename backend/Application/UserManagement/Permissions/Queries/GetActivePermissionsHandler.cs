using Application.Common.BaseDTO;
using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetActivePermissionsHandler(IPermissionService permissionService) : IRequestHandler<GetActivePermissionsQuery, ValidationResult<PagedResponseDTO<PermissionDto>>>
{
    public async Task<ValidationResult<PagedResponseDTO<PermissionDto>>> Handle(GetActivePermissionsQuery request, CancellationToken cancellationToken)
    {
        return await permissionService.GetActiveEntitiesAsync(request.PageNumber, request.PageSize, request.OrderBy, request.IsDescending, cancellationToken);
    }
}