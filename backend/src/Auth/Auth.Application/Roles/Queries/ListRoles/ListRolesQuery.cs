namespace Auth.Application.Roles.Queries.ListRoles;

using Auth.Application.Roles.DTOs;
using Core.API;

/// <summary>
/// Query to list all roles with optional pagination and filtering.
/// </summary>
public sealed record ListRolesQuery : BasePagedQuery<RoleDTO>;
