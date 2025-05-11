using Domain.CatalogManagement;
using Domain.LoanManagement.Enums;
using Domain.SeedWork;
using Domain.UserManagement;

namespace Domain.LoanManagement;

public class BookCheckout : RelationalDbBaseModel
{
    public required User User {get;set;}
    public required Book Book { get; set; }
    public required DateTime CheckoutDate { get; set; }
    public required DateOnly DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Notes { get; set; }
    public required CheckoutStatusEnum Status { get; set; }
}