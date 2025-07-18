using Application.Common.FluentValidation;
using Application.UserManagement.Permissions.DTOs;

namespace Application.UserManagement.Permissions.Queries;

public class GetActivePermissionsValidator : BasePagedQueryValidator<GetActivePermissionsQuery, PermissionDto>;