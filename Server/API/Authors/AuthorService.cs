using LibraryApp.API.Gateway;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.API.Authors;

public class AuthorService(SqlDataContext sqlDataContext) : IAuthorService
{
    private readonly SqlDataContext sqlDataContext = sqlDataContext;

    public async Task<IList<Author>> GetAll()
    {
        return await sqlDataContext.Authors
            .OrderBy(a => a.FirstName)
            .ToListAsync();
    }
}

public interface IAuthorService
{
    Task<IList<Author>> GetAll();
}