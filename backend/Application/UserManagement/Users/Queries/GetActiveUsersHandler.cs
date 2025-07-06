using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using FluentResults;
using MediatR;

namespace Application.UserManagement.Users.Queries;

public class GetActiveUsersHandler(IUserService userService) : IRequestHandler<GetActiveUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        var userResults = await userService.GetActiveUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return userResults.IsSuccess
            ? Result.Ok(userResults.Value.Select(user => (UserDto)user))
            : Result.Fail<IEnumerable<UserDto>>(userResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve users.");
    }
}