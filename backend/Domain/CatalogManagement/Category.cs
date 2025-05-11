using Domain.SeedWork;

namespace Domain.CatalogManagement;

public class Category : RelationalDbBaseModel
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public IList<Book> Books { get; set; } = [];
}