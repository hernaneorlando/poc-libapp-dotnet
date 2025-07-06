using Application.CatalogManagement.Books.DTOs;
using Domain.CatalogManagement;
using FluentResults;

namespace Application.CatalogManagement.Books.Services;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> GetCategoryDtoByIdAsync(Guid guid, CancellationToken cancellationToken);
    Task<Result<Category>> GetCategoryByIdAsync(Guid guid, CancellationToken cancellationToken);

    Task<Result<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<Result<Category>> UpdateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<Result> DeleteCategoryAsync(Guid externalId, CancellationToken cancellationToken);
}
