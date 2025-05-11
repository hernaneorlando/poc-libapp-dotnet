using Domain.CatalogManagement;
using FluentResults;

namespace Application.CatalogManagement.Books.Services;

public interface IBookService
{
    Task<Result<IEnumerable<Book>>> GetActiveBooksAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
