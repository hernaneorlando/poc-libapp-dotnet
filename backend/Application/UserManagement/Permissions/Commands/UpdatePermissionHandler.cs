using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.UserManagement;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public class UpdatePermissionHandler(IPermissionService permissionService) : IRequestHandler<UpdatePermissionCommand, Result<PermissionDto>>
{
    public async Task<Result<PermissionDto>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = (Permission)request;
        var permissionResult = await permissionService.UpdatePermissionAsync(permission, cancellationToken);
        return permissionResult.IsSuccess
            ? Result.Ok((PermissionDto)permissionResult.Value)
            : Result.Fail<PermissionDto>(permissionResult.Errors.FirstOrDefault()?.Message ?? "Failed to update permission.");
    }
}