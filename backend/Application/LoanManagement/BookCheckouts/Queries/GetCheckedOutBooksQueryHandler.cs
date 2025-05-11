using Application.LoanManagement.BookCheckouts.DTOs;
using Application.LoanManagement.BookCheckouts.Services;
using FluentResults;
using MediatR;

namespace Application.LoanManagement.BookCheckouts.Queries;

public class GetCheckedOutBooksQueryHandler(IBookCheckoutService bookCheckoutService) : IRequestHandler<GetCheckedOutBooksQuery, Result<IEnumerable<BookCheckoutDto>>>
{
    private readonly IBookCheckoutService bookCheckoutService = bookCheckoutService;

    public async Task<Result<IEnumerable<BookCheckoutDto>>> Handle(GetCheckedOutBooksQuery request, CancellationToken cancellationToken)
    {
        var checkoutResults = await bookCheckoutService.GetCheckedOutBooksAsync(request.PageNumber, request.PageSize, cancellationToken);
        return checkoutResults.IsSuccess
            ? Result.Ok(checkoutResults.Value.Select(checkout => (BookCheckoutDto)checkout))
            : Result.Fail<IEnumerable<BookCheckoutDto>>(checkoutResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve checked out books.");
    }
}