using Application.SeedWork.FluentValidation;
using Application.UserManagement.Roles.DTOs;

namespace Application.UserManagement.Roles.Queries;

public class GetActiveRolesQueryValidator : BasePagedQueryValidator<GetActiveRolesQuery, RoleDto>;