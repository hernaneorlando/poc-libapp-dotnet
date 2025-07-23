using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Users.Queries;

public class GetActiveUsersHandler(IUserService userService) : IRequestHandler<GetActiveUsersQuery, ValidationResult<IEnumerable<UserDto>>>
{
    public async Task<ValidationResult<IEnumerable<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        var userResults = await userService.GetActiveUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return userResults.IsSuccess
            ? ValidationResult.Ok(userResults.Value.Select(user => (UserDto)user))
            : ValidationResult.Fail<IEnumerable<UserDto>>(userResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve users.");
    }
}