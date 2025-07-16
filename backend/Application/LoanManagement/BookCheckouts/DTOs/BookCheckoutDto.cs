using Application.CatalogManagement.Books.DTOs;
using Application.Common.BaseDTO;
using Application.UserManagement.Users.DTOs;
using Domain.LoanManagement;
using Domain.LoanManagement.Enums;

namespace Application.LoanManagement.BookCheckouts.DTOs;

public record BookCheckoutDto(UserDto User, BookDto Book, DateTime CheckoutDate, DateOnly DueDate, CheckoutStatusEnum Status) : BaseDto
{
    public DateTime? ReturnDate { get; set; }
    public string? Notes { get; set; }

    public static implicit operator BookCheckoutDto(BookCheckout bookCheckout)
    {
        var bookCheckoutDto = new BookCheckoutDto(
            (UserDto)bookCheckout.User,
            (BookDto)bookCheckout.Book,
            bookCheckout.CheckoutDate,
            bookCheckout.DueDate,
            bookCheckout.Status)
        {
            ReturnDate = bookCheckout.ReturnDate,
            Notes = bookCheckout.Notes
        };

        bookCheckoutDto.ConvertBaseProperties(bookCheckout);
        return bookCheckoutDto;
    }
}