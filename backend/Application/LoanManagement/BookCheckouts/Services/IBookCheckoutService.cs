using Domain.LoanManagement;
using FluentResults;

namespace Application.LoanManagement.BookCheckouts.Services;

public interface IBookCheckoutService
{
    Task<Result<IEnumerable<BookCheckout>>> GetCheckedOutBooksAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}