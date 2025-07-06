using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetPermissionByIdHandler(IPermissionService permissionService) : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
{
    public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permissionResult = await permissionService.GetPermissionByIdAsync(Guid.Parse(request.Id), cancellationToken);
        return permissionResult.IsSuccess
            ? Result.Ok(permissionResult.Value)
            : Result.Fail<PermissionDto>(permissionResult.Errors.FirstOrDefault()?.Message ?? $"Permission with ID {request.Id} not found.");
    }
}