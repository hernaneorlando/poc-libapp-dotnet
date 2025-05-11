using Application.LoanManagement.BookCheckouts.Services;
using Application.UserManagement.Users.Services;
using Domain.LoanManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class BookCheckoutService(SqlDataContext _sqlContext, IUserService _userService) : IBookCheckoutService
{
    public async Task<Result<IEnumerable<BookCheckout>>> GetCheckedOutBooksAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var checkouts = await _sqlContext.BookCheckouts
            .Where(c => c.Active)
            .OrderBy(c => c.CheckoutDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (checkouts is null || checkouts.Count == 0)
        {
            return Result.Fail("No checked out books found.");
        }

        var usersResult = await _userService.GetUsersByIdsAsync(checkouts.Select(c => c.UserId), cancellationToken);
        if (usersResult.IsFailed)
        {
            return Result.Fail(usersResult.Errors);
        }

        var userDictionary = usersResult.Value.ToDictionary(u => u.Id, u => u);
        var bookCheckouts = checkouts.Select(checkoutEntity => 
        {
            var checkout = (BookCheckout)checkoutEntity;
            checkout.User = userDictionary[checkoutEntity.UserId];
            return checkout;
        });

        return Result.Ok(bookCheckouts);
    }
}
