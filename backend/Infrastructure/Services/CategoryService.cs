using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CategoryService(SqlDataContext sqlDataContext) : ICategoryService
{
    private readonly SqlDataContext sqlDataContext = sqlDataContext;

    public async Task<Result<IEnumerable<Category>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var categories = await sqlDataContext.Categories
            .Where(c => c.Active)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (categories is null || categories.Count == 0)
        {
            return Result.Fail("No active categories found.");
        }

        return Result.Ok(categories.Select(category => (Category)category));
    }
}