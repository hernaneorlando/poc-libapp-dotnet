using Application.Common.MediatR;
using Application.UserManagement.Roles.DTOs;

namespace Application.UserManagement.Roles.Queries;

public record GetActiveRolesQuery : BasePagedQuery<RoleDto>;