using Application.Common.BaseDTO;
using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using Domain.Common;
using MediatR;

namespace Application.UserManagement.Users.Queries;

public class GetActiveUsersHandler(IUserService userService) : IRequestHandler<GetActiveUsersQuery, ValidationResult<PagedResponseDTO<UserDto>>>
{
    public async Task<ValidationResult<PagedResponseDTO<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        var userResults = await userService.GetActiveUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return userResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<UserDto> { Data = [..userResults.Value.Select(user => (UserDto)user)] })
            : ValidationResult.Fail<PagedResponseDTO<UserDto>>(userResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve users.");
    }
}