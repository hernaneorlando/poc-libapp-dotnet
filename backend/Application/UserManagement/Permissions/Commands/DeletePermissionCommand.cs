using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public record DeletePermissionCommand(string Id) :IRequest<Result>;