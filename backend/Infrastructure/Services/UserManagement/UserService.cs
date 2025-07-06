using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Services;
using Domain.UserManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using MongoDB.Driver;

namespace Infrastructure.Services.UserManagement;

public class UserService(NoSqlDataContext noSqlDataContext) : IUserService
{
    public async Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await noSqlDataContext.Users
            .Find(x => x.ExternalId.ToString() == id.ToString())
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Fail("User not found.");
        }

        return Result.Ok((UserDto)user);
    }

    public async Task<Result<IEnumerable<User>>> GetActiveUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var users = await noSqlDataContext.Users
            .Find(x => x.Active)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        if (users is null || users.Count == 0)
        {
            return Result.Fail("No active users found.");
        }

        return Result.Ok(users.Select(user => (User)user));
    }

    public async Task<Result<IEnumerable<User>>> GetUsersByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var filter = Builders<UserEntity>.Filter.In(x => x.Id, userIds);
        var users = await noSqlDataContext.Users
            .Find(filter)
            .ToListAsync(cancellationToken);

        if (users is null || users.Count == 0)
        {
            return Result.Fail("No users found with the provided IDs.");
        }

        return Result.Ok(users.Select(user => (User)user));
    }
}
