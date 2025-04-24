using Application.CatalogManagement.Authors.Services;
using Domain.CatalogManagement;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

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

