using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Application.Common.BaseDTO;
using Domain.CatalogManagement;
using Domain.Common;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.CatalogManagement;

public class CategoryService(SqlDataContext sqlDataContext) : ICategoryService
{
    #region Query Methods

    public async Task<ValidationResult<CategoryDto>> GetCategoryDtoByIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(externalId, cancellationToken);
        return categoryEntity is not null
            ? ValidationResult.Ok((CategoryDto)categoryEntity)
            : ValidationResult.Fail<CategoryDto>($"Category with ID {externalId} not found.");
    }

    public async Task<ValidationResult<Category>> GetCategoryByIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var categoryEntity = await GetByExternalId(externalId, cancellationToken);
        return categoryEntity is not null
            ? ValidationResult.Ok((Category)categoryEntity)
            : ValidationResult.Fail<Category>($"Category with ID {externalId} not found.");
    }

    private async Task<CategoryEntity?> GetByExternalId(Guid externalId, CancellationToken cancellationToken)
    {
        return await sqlDataContext.Categories
            .AsNoTracking()
            .Where(c => c.Active && c.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValidationResult<PagedResponseDTO<CategoryDto>>> GetActiveEntitiesAsync(int pageNumber, int pageSize, string? orderBy = null, bool? isDescending = null, CancellationToken? cancellationToken = null)
    {
        var query = sqlDataContext.Categories
            .Where(c => c.Active);

        query = isDescending == true
            ? query.OrderByDescending(c => string.IsNullOrWhiteSpace(orderBy) ? c.Name : EF.Property<object>(c, orderBy!) ?? c.Name)
            : query.OrderBy(c => string.IsNullOrWhiteSpace(orderBy) ? c.Name : EF.Property<object>(c, orderBy!) ?? c.Name);

        var totalRecords = await query.CountAsync();

        var categories = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken ?? CancellationToken.None);

        if (categories.Count == 0)
            return ValidationResult.Fail<PagedResponseDTO<CategoryDto>>("No active categories found.");

        var response = new PagedResponseDTO<CategoryDto>
        {
            Data = [..categories.Select(category => (CategoryDto)category)],
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            CurrentPage = pageNumber,
            PageSize = pageSize
        };

        return ValidationResult.Ok(response);
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
