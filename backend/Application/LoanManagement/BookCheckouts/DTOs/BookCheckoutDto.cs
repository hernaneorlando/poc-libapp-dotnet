using Application.CatalogManagement.Books.DTOs;
using Application.Common;
using Application.SeedWork.BaseDTO;
using Application.UserManagement.Users.DTOs;
using Domain.CatalogManagement;
using Domain.LoanManagement;
using Domain.LoanManagement.Enums;
using Domain.UserManagement;

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

        bookCheckoutDto.ConvertModelBaseProperties(bookCheckout);
        return bookCheckoutDto;
    }

    public static implicit operator BookCheckout(BookCheckoutDto bookCheckoutDto)
    {
        var bookCheckout = new BookCheckout
        {
            User = (User)bookCheckoutDto.User,
            Book = (Book)bookCheckoutDto.Book,
            Status = bookCheckoutDto.Status,
            CheckoutDate = bookCheckoutDto.CheckoutDate,
            DueDate = bookCheckoutDto.DueDate,
            ReturnDate = bookCheckoutDto.ReturnDate,
            Notes = bookCheckoutDto.Notes
        };

        bookCheckout.ConvertDtoBaseProperties(bookCheckoutDto);
        return bookCheckout;
    }
}