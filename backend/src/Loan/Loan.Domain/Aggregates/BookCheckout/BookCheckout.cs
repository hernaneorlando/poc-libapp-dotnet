using Loan.Domain.Enums;

namespace Loan.Domain.Aggregates.BookCheckout;

public class BookCheckout
{
    public required UserId UserId { get; set; }
    public required BookId BookId { get; set; }
    public required DateTime CheckoutDate { get; set; }
    public required DateOnly DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Notes { get; set; }
    public required CheckoutStatusEnum Status { get; set; }
}
