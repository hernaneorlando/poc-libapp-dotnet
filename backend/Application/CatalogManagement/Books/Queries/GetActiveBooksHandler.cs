using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveBooksHandler(IBookService bookService) : IRequestHandler<GetActiveBooksQuery, ValidationResult<PagedResponseDTO<BookDto>>>
{
    private readonly IBookService bookService = bookService;

    public async Task<ValidationResult<PagedResponseDTO<BookDto>>> Handle(GetActiveBooksQuery request, CancellationToken cancellationToken)
    {
        var bookResults = await bookService.GetActiveBooksAsync(request.PageNumber, request.PageSize, cancellationToken);
        return bookResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<BookDto> { Data = [..bookResults.Value.Select(book => (BookDto)book)] })
            : ValidationResult.Fail<PagedResponseDTO<BookDto>>(bookResults.Errors.FirstOrDefault()?.Message ?? "No active books found.");
    }
}