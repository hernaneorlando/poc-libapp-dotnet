using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using Domain.UserManagement;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetPermissionByIdHandler(IPermissionService permissionService) : IRequestHandler<GetPermissionByIdQuery, ValidationResult<PermissionDto>>
{
    public async Task<ValidationResult<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var externalIdResult = Permission.ParseExternalId(request.Id);
        if (externalIdResult.IsFailure)
            return ValidationResult.Fail<PermissionDto>(externalIdResult.Errors);

        return await permissionService.GetPermissionDtoByIdAsync(Guid.Parse(request.Id), cancellationToken);
    }
}