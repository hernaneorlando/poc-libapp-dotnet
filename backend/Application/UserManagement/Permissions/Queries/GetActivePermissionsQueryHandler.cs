using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Queries;

public class GetActivePermissionsQueryHandler(IPermissionService permissionService) : IRequestHandler<GetActivePermissionsQuery, Result<IEnumerable<PermissionDto>>>
{
    private readonly IPermissionService permissionService = permissionService;

    public async Task<Result<IEnumerable<PermissionDto>>> Handle(GetActivePermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissionResults = await permissionService.GetActivePermissionsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return permissionResults.IsSuccess
            ? Result.Ok(permissionResults.Value.Select(permission => (PermissionDto)permission))
            : Result.Fail<IEnumerable<PermissionDto>>(permissionResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve permissions.");
    }
}