using Domain.LoanManagement;
using Domain.LoanManagement.Enums;
using Domain.UserManagement;
using Infrastructure.Common;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class BookCheckoutEntity : RelationalDbBaseBaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public DateTime CheckoutDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Notes { get; set; }
    public CheckoutStatusEnum Status { get; set; }

    public BookEntity Book { get; set; } = new();
    public long BookId { get; set; }

    public static implicit operator BookCheckout(BookCheckoutEntity entity)
    {
        var model = new BookCheckout
        {
            User = new User(entity.UserId),
            Book = entity.Book,
            CheckoutDate = entity.CheckoutDate,
            DueDate = entity.DueDate,
            ReturnDate = entity.ReturnDate,
            Notes = entity.Notes,
            Status = entity.Status,
        };

        model.ConvertEntityBaseProperties(entity);        
        return model;
    }

    public static explicit operator BookCheckoutEntity(BookCheckout bookCheckout)
    {
        var entity = new BookCheckoutEntity
        {
            UserId = bookCheckout.User.Id,
            Book = bookCheckout.Book,
            CheckoutDate = bookCheckout.CheckoutDate,
            DueDate = bookCheckout.DueDate,
            ReturnDate = bookCheckout.ReturnDate,
            Notes = bookCheckout.Notes,
            Status = bookCheckout.Status,
        };

        entity.ConvertModelBaseProperties(bookCheckout);
        return entity;
    }
}