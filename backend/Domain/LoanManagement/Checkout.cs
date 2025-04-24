using Domain.CatalogManagement;
using Domain.Shared;

namespace Domain.LoanManagement;

public class Checkout : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Book Book { get; set; } = new Book();
}