using LibraryApp.API.Gateway;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.API.Authors;

public class AuthorService : IAuthorService
{
    private readonly SqlDataContext sqlDataContext;

    public AuthorService(SqlDataContext sqlDataContext)
    {
        this.sqlDataContext = sqlDataContext;
    }

    public async Task<IList<Author>> FindAll()
    {
        return await sqlDataContext.Authors
            .OrderBy(a => a.FirstName)
            .ToListAsync();
    }
}

public interface IAuthorService
{
    Task<IList<Author>> FindAll();
}