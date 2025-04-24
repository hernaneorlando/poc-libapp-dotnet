using Application.UserManagement.Users.Services;
using Domain.UserManagement;
using Infrastructure.Persistence.Context;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class UserService(NoSqlDataContext noSqlDataContext) : IUserService
{
    private readonly NoSqlDataContext noSqlDataContext = noSqlDataContext;

    public async Task<IList<User>> GetAll()
    {
        var result = await noSqlDataContext.Users.FindAsync(_ => true);
        return await result.ToListAsync();
    }
}

