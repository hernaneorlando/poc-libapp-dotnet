using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveBooksHandler(IBookService bookService) : IRequestHandler<GetActiveBooksQuery, ValidationResult<IEnumerable<BookDto>>>
{
    private readonly IBookService bookService = bookService;

    public async Task<ValidationResult<IEnumerable<BookDto>>> Handle(GetActiveBooksQuery request, CancellationToken cancellationToken)
    {
        var bookResults = await bookService.GetActiveBooksAsync(request.PageNumber, request.PageSize, cancellationToken);
        return bookResults.IsSuccess
            ? ValidationResult.Ok(bookResults.Value.Select(book => (BookDto)book))
            : ValidationResult.Fail<IEnumerable<BookDto>>(bookResults.Errors.FirstOrDefault()?.Message ?? "No active books found.");
    }
}