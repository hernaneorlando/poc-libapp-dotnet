using Domain.CatalogManagement.Enums;
using Domain.CatalogManagement.ValueObjects;
using Domain.LoanManagement;
using Domain.SeedWork;

namespace Domain.CatalogManagement;

public class Book : RelationalDbBaseModel
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