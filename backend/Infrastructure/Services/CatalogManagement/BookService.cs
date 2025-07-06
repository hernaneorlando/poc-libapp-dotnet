using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.CatalogManagement;

public class BookService(SqlDataContext sqlDataContext) : IBookService
{
    public async Task<Result<IEnumerable<Book>>> GetActiveBooksAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var books = await sqlDataContext.Books
            .Where(c => c.Active)
            .OrderBy(c => c.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        if (books is null || books.Count == 0)
        {
            return Result.Fail("No active books found.");
        }
        
        return Result.Ok(books.Select(book => (Book)book));
    }
}
