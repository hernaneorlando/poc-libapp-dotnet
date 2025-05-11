using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using Domain.UserManagement;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public class CreatePermissionCommandHandler(IPermissionService permissionService) : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permissio = (Permission)request;
        var permissionResult = await permissionService.CreatePermissionAsync(permissio, cancellationToken);
        return permissionResult.IsSuccess
            ? Result.Ok((PermissionDto)permissionResult.Value)
            : Result.Fail<PermissionDto>(permissionResult.Errors.FirstOrDefault()?.Message ?? "Failed to create permission.");
    }
}