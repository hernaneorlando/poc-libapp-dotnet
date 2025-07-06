using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Users.Queries;

public class GetUserByIdHandler(IUserService userService) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userResult = await userService.GetUserByIdAsync(Guid.Parse(request.Id), cancellationToken);
        return userResult.IsSuccess
            ? Result.Ok(userResult.Value)
            : Result.Fail<UserDto>(userResult.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve user.");
    }
}