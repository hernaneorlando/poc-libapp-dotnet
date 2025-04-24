using Domain.LoanManagement;
using Domain.Shared;

namespace Domain.CatalogManagement;

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalPages { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public Category? Category { get; set; }
    public Publisher Publisher { get; set; } = new Publisher();
    public Author? MainAuthor { get; set; }
    public ICollection<Author> Authors { get; set; } = [];
    public ICollection<Checkout> Checkouts { get; set; } = [];
}