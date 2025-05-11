using Domain.SeedWork;

namespace Domain.CatalogManagement;

public class Contributor : RelationalDbBaseModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; } 
    public DateOnly? DateOfBirth { get; set; }
    public ICollection<BookContributor> Books { get; set; } = [];
}