using Domain.CatalogManagement.Enums;
using Domain.CatalogManagement.ValueObjects;
using Domain.Common;
using Domain.LoanManagement;

namespace Domain.CatalogManagement;

public class Book : RelationalDbModel<Book>
{
    public required string Title { get; set; }
    public Isbn? ISBN { get; set; }
    public string? Description { get; set; }
    public int? Edition { get; set; }
    public string? Language { get; set; }
    public int? TotalPages { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public required BookStatusEnum Status { get; set; }

    public Category? Category { get; set; }
    public required Publisher Publisher { get; set; }
    public ICollection<BookContributor> Contributors { get; set; } = [];
    public ICollection<BookCheckout> Checkouts { get; set; } = [];
}