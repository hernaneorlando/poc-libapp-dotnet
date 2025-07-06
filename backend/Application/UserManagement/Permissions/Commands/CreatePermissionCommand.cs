using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public record CreatePermissionCommand(string Code, string Description) : IRequest<Result<PermissionDto>>
{
    public static implicit operator Permission(CreatePermissionCommand command) =>
        new()
        {
            Code = command.Code,
            Description = command.Description
        };
}