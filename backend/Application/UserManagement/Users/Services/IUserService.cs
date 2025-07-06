using Application.UserManagement.Users.DTOs;
using Domain.UserManagement;
using FluentResults;

namespace Application.UserManagement.Users.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetUserByIdAsync(Guid Id, CancellationToken cancellationToken);
    Task<Result<IEnumerable<User>>> GetActiveUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Result<IEnumerable<User>>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken);
}