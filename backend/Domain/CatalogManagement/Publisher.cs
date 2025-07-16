using Domain.CatalogManagement.ValueObjects;
using Domain.Common;

namespace Domain.CatalogManagement;

public class Publisher : RelationalDbBaseModel<Publisher>
{
    public required string Name { get; set; }
    public DateOnly? FoundationDate { get; set; }
    public PublisherContact? Contact { get; set; }
    public IList<Book> Books { get; set; } = [];
}