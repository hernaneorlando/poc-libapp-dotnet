using LibraryApp.API.Gateway;
using MongoDB.Driver;

namespace LibraryApp.API.Users;

public class UserService(NoSqlDataContext noSqlDataContext) : IUserService
{
    private readonly NoSqlDataContext noSqlDataContext = noSqlDataContext;

    public async Task<IList<User>> GetAll()
    {
        var result = await noSqlDataContext.Users.FindAsync(_ => true);
        return await result.ToListAsync();
    }
}

public interface IUserService
{
    Task<IList<User>> GetAll();
}