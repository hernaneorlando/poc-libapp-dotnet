using Application.CatalogManagement.Books.DTOs;
using Domain.CatalogManagement;
using Domain.Common;

namespace Application.CatalogManagement.Books.Services;

public interface ICategoryService
{
    Task<ValidationResult<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<ValidationResult<CategoryDto>> GetCategoryDtoByIdAsync(Guid guid, CancellationToken cancellationToken);
    Task<ValidationResult<Category>> GetCategoryByIdAsync(Guid guid, CancellationToken cancellationToken);

    Task<ValidationResult<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<ValidationResult<Category>> UpdateCategoryAsync(Category category, CancellationToken cancellationToken);
}
