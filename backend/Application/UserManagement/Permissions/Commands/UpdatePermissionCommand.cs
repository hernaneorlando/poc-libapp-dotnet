using System.Text.Json.Serialization;
using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public record UpdatePermissionCommand(string Description) : IRequest<Result<PermissionDto>>
{
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;

    public static implicit operator Permission(UpdatePermissionCommand command) =>
        new()
        {
            Code = string.Empty,
            Description = command.Description,
            ExternalId = Guid.TryParse(command.Id, out var externalId) ? externalId : Guid.Empty
        };
}
