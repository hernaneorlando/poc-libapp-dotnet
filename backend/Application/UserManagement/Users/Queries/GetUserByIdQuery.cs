using Application.Common.MediatR;
using Application.UserManagement.Users.DTOs;

namespace Application.UserManagement.Users.Queries;

public record GetUserByIdQuery : BaseGetByIdQuery<UserDto>;