using Application.UserManagement.Users.Services;
using Domain.UserManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class UserService(NoSqlDataContext noSqlDataContext) : IUserService
{
    private readonly NoSqlDataContext noSqlDataContext = noSqlDataContext;

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

