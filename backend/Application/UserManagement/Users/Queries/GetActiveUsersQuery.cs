using Application.SeedWork.MediatR;
using Application.UserManagement.Users.DTOs;

namespace Application.UserManagement.Users.Queries;

public record GetActiveUsersQuery : BasePagedQuery<UserDto>;