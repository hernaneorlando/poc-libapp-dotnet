using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.CatalogManagement;

public class CategoryService(SqlDataContext sqlDataContext) : ICategoryService
{
    #region Query Methods

    public async Task<Result<CategoryDto>> GetCategoryDtoByIdAsync(Guid guid, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(guid, cancellationToken);
        return categoryEntity is not null
            ? Result.Ok((CategoryDto)categoryEntity)
            : Result.Fail<CategoryDto>($"Category with ID {guid} not found.");
    }

    public async Task<Result<Category>> GetCategoryByIdAsync(Guid guid, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(guid, cancellationToken);
        return categoryEntity is not null
            ? Result.Ok((Category)categoryEntity)
            : Result.Fail<Category>($"Category with ID {guid} not found.");
    }

    private async Task<CategoryEntity?> GetByExternalId(Guid guid, CancellationToken cancellationToken)
    {
        return await sqlDataContext.Categories
            .Where(c => c.Active && c.ExternalId.ToString() == guid.ToString())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
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

        return Result.Ok(categories.Select(category => (CategoryDto)category));
    }

    #endregion

    #region Command Methods

    public async Task<Result<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        var entry = sqlDataContext.Categories.Add(category);
        await sqlDataContext.SaveChangesAsync(cancellationToken);
        return Result.Ok((Category)entry.Entity);
    }

    public async Task<Result<Category>> UpdateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        var categoryEntity = (CategoryEntity)category;
        sqlDataContext.Entry(categoryEntity).State = EntityState.Modified;
        var entry = sqlDataContext.Categories.Update(categoryEntity);
        await sqlDataContext.SaveChangesAsync(cancellationToken);
        return Result.Ok((Category)entry.Entity);
    }

    public async Task<Result> DeleteCategoryAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var persistedCategory = await sqlDataContext.Categories
            .Where(c => c.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (persistedCategory == null)
        {
            return Result.Fail("Category not found.");
        }

        persistedCategory.Active = false;
        sqlDataContext.Categories.Update(persistedCategory);
        await sqlDataContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
    
    #endregion
}
