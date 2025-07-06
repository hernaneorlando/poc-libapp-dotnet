using Application.UserManagement.Permissions.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public class DeletePermissionHandler(IPermissionService permissionCommandService) : IRequestHandler<DeletePermissionCommand, Result>
{
    public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var result = await permissionCommandService.DeletePermissionAsync(Guid.Parse(request.Id), cancellationToken);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        return Result.Ok();
    }
}