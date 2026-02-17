namespace Auth.Application.Users.Commands.CreateUser;

using MediatR;
using Auth.Application.Users.DTOs;
using Core.API;

/// <summary>
/// Command to create a new user in the system with assigned roles.
/// </summary>
public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string UserType,
    string? PhoneNumber = null,
    IReadOnlyList<string>? RoleIds = null
) : IRequest<Result<UserDTO>>;
