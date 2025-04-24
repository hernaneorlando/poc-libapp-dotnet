using Domain.Shared;

namespace Domain.CatalogManagement;

public class Author : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; }  = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public ICollection<Book> Books { get; set; } = [];
}