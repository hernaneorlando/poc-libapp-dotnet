using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.Common;
using Domain.UserManagement;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public class UpdatePermissionHandler(IPermissionService permissionService) : IRequestHandler<UpdatePermissionCommand, ValidationResult<PermissionDto>>
{
    public async Task<ValidationResult<PermissionDto>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permissionIdResult = Permission.ParseExternalId(request.Id);
        if (permissionIdResult.IsFailure)
            return ValidationResult.Fail<PermissionDto>(permissionIdResult.Errors);

        var persistedPermissionResult = await permissionService.GetPermissionByIdAsync(permissionIdResult.Value, cancellationToken);
        if (persistedPermissionResult.IsFailure)
            return ValidationResult.Fail<PermissionDto>(persistedPermissionResult.Errors);
            
        var persistedPermission = persistedPermissionResult.Value;
        var result = persistedPermission.Update(permissionIdResult.Value, request.Description);
        if (result.IsFailure)
            return ValidationResult.Fail<PermissionDto>(result.Errors);

        var PermissionUpdatedResult = await permissionService.UpdatePermissionAsync(result.Value, cancellationToken);
        return PermissionUpdatedResult.IsSuccess
            ? ValidationResult.Ok((PermissionDto)PermissionUpdatedResult.Value)
            : ValidationResult.Fail<PermissionDto>(PermissionUpdatedResult.Errors);
    }
}