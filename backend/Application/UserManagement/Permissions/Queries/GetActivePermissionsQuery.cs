using Application.Common.MediatR;
using Application.UserManagement.Permissions.DTOs;

namespace Application.UserManagement.Permissions.Queries;

public record GetActivePermissionsQuery : BasePagedQuery<PermissionDto>;
