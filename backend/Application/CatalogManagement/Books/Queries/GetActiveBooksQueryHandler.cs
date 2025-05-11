using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveBooksQueryHandler(IBookService bookService) : IRequestHandler<GetActiveBooksQuery, Result<IEnumerable<BookDto>>>
{
    private readonly IBookService bookService = bookService;

    public async Task<Result<IEnumerable<BookDto>>> Handle(GetActiveBooksQuery request, CancellationToken cancellationToken)
    {
        var bookResults = await bookService.GetActiveBooksAsync(request.PageNumber, request.PageSize, cancellationToken);
        return bookResults.IsSuccess
            ? Result.Ok(bookResults.Value.Select(book => (BookDto)book))
            : Result.Fail<IEnumerable<BookDto>>(bookResults.Errors.FirstOrDefault()?.Message ?? "No active books found.");
    }
}