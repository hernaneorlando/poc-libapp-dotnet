using System.Text.Json.Serialization;
using Application.UserManagement.Permissions.DTOs;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Permissions.Commands;

public record UpdatePermissionCommand : IRequest<ValidationResult<PermissionDto>>
{
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
