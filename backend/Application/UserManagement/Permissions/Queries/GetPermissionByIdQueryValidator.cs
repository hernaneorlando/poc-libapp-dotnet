using Application.SeedWork.FluentValidation;
using Application.UserManagement.Permissions.DTOs;

namespace Application.UserManagement.Permissions.Queries;

public class GetPermissionByIdQueryValidator : BaseGetByIdQueryValidator<GetPermissionByIdQuery, PermissionDto>;