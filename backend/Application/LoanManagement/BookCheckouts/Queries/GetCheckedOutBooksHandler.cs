using Application.LoanManagement.BookCheckouts.DTOs;
using Application.LoanManagement.BookCheckouts.Services;
using Domain.Common;
using MediatR;

namespace Application.LoanManagement.BookCheckouts.Queries;

public class GetCheckedOutBooksHandler(IBookCheckoutService bookCheckoutService) : IRequestHandler<GetCheckedOutBooksQuery, ValidationResult<IEnumerable<BookCheckoutDto>>>
{
    public async Task<ValidationResult<IEnumerable<BookCheckoutDto>>> Handle(GetCheckedOutBooksQuery request, CancellationToken cancellationToken)
    {
        var checkoutResults = await bookCheckoutService.GetCheckedOutBooksAsync(request.PageNumber, request.PageSize, cancellationToken);
        return checkoutResults.IsSuccess
            ? ValidationResult.Ok(checkoutResults.Value.Select(checkout => (BookCheckoutDto)checkout))
            : ValidationResult.Fail<IEnumerable<BookCheckoutDto>>(checkoutResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve checked out books.");
    }
}