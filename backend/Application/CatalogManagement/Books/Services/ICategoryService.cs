using Domain.CatalogManagement;
using FluentResults;

namespace Application.CatalogManagement.Books.Services;

public interface ICategoryService
{
    Task<Result<IEnumerable<Category>>> GetActiveCategoriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
