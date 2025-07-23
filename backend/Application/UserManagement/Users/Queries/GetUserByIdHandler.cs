using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Users.Queries;

public class GetUserByIdHandler(IUserService userService) : IRequestHandler<GetUserByIdQuery, ValidationResult<UserDto>>
{
    public async Task<ValidationResult<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userResult = await userService.GetUserByIdAsync(Guid.Parse(request.Id), cancellationToken);
        return userResult.IsSuccess
            ? ValidationResult.Ok(userResult.Value)
            : ValidationResult.Fail<UserDto>(userResult.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve user.");
    }
}