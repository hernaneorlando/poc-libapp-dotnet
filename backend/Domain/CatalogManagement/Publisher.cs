using Domain.CatalogManagement.ValueObjects;
using Domain.SeedWork;

namespace Domain.CatalogManagement;

public class Publisher : RelationalDbBaseModel
{
    public required string Name { get; set; }
    public DateOnly? FoundationDate { get; set; }
    public Contact? Contact { get; set; }
    public IList<Book> Books { get; set; } = [];
}