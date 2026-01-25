namespace Auth.Application.Roles.Queries.ListRoles;

using Auth.Application.Roles.DTOs;
using Core.Application;

/// <summary>
/// Validator for ListRolesQuery.
/// Ensures pagination parameters are valid.
/// </summary>
public sealed class ListRolesQueryValidator : BasePagedQueryValidator<ListRolesQuery, RoleDTO>;