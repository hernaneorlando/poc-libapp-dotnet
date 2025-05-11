using Application.SeedWork.FluentValidation;
using Application.UserManagement.Users.DTOs;

namespace Application.UserManagement.Users.Queries;

public class GetActiveUsersQueryValidator : BasePagedQueryValidator<GetActiveUsersQuery, UserDto>;