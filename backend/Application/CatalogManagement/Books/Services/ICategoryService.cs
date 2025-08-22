using Application.CatalogManagement.Books.DTOs;
using Application.Common;
using Domain.CatalogManagement;
using Domain.Common;

namespace Application.CatalogManagement.Books.Services;

public interface ICategoryService : IPagedResponseService<CategoryDto>
{
    Task<ValidationResult<CategoryDto>> GetCategoryDtoByIdAsync(Guid guid, CancellationToken cancellationToken);
    Task<ValidationResult<Category>> GetCategoryByIdAsync(Guid guid, CancellationToken cancellationToken);

    Task<ValidationResult<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<ValidationResult<Category>> UpdateCategoryAsync(Category category, CancellationToken cancellationToken);
}
