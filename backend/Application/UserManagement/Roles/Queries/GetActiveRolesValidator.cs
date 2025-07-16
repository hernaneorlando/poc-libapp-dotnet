using Application.Common.FluentValidation;
using Application.UserManagement.Roles.DTOs;

namespace Application.UserManagement.Roles.Queries;

public class GetActiveRolesValidator : BasePagedQueryValidator<GetActiveRolesQuery, RoleDto>;