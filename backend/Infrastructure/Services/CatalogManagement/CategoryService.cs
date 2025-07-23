using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.Common;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.CatalogManagement;

public class CategoryService(SqlDataContext sqlDataContext) : ICategoryService
{
    #region Query Methods

    public async Task<ValidationResult<CategoryDto>> GetCategoryDtoByIdAsync(Guid guid, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(guid, cancellationToken);
        return categoryEntity is not null
            ? ValidationResult.Ok((CategoryDto)categoryEntity)
            : ValidationResult.Fail<CategoryDto>($"Category with ID {guid} not found.");
    }

    public async Task<ValidationResult<Category>> GetCategoryByIdAsync(Guid guid, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(guid, cancellationToken);
        return categoryEntity is not null
            ? ValidationResult.Ok((Category)categoryEntity)
            : ValidationResult.Fail<Category>($"Category with ID {guid} not found.");
    }

    private async Task<CategoryEntity?> GetByExternalId(Guid guid, CancellationToken cancellationToken)
    {
        return await sqlDataContext.Categories
            .AsNoTracking()
            .Where(c => c.Active && c.ExternalId == guid)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValidationResult<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var categories = await sqlDataContext.Categories
            .Where(c => c.Active)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return categories.Count != 0
            ? ValidationResult.Ok(categories.Select(category => (CategoryDto)category))
            : ValidationResult.Fail<IEnumerable<CategoryDto>>("No active categories found.");
    }

    #endregion

    #region Command Methods

    public async Task<ValidationResult<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        var entry = sqlDataContext.Categories.Add(category);
        var entriesCount = await sqlDataContext.SaveChangesAsync(cancellationToken);
        return entriesCount == 1
            ? ValidationResult.Ok((Category)entry.Entity)
            : ValidationResult.Fail<Category>("Failed to create Category.");
    }

    public async Task<ValidationResult<Category>> UpdateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        var categoryEntity = (CategoryEntity)category;
        sqlDataContext.Entry(categoryEntity).State = EntityState.Modified;
        var entry = sqlDataContext.Categories.Update(categoryEntity);
        var entriesCount = await sqlDataContext.SaveChangesAsync(cancellationToken);
        return entriesCount == 1
            ? ValidationResult.Ok((Category)entry.Entity)
            : ValidationResult.Fail<Category>("Failed to update Category.");
    }

    #endregion
}
