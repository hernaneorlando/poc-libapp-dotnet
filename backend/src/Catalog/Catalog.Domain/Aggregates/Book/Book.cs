namespace Catalog.Domain.Aggregates.Book;

using Catalog.Domain.Aggregates.Contributor;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Aggregates.Publisher;

/// <summary>
/// Aggregate Root representing a Book in the catalog.
/// Manages book data, availability, and relationships with contributors, publishers, and categories.
/// </summary>
public sealed class Book : AggregateRoot<BookId>
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public IsbnNumber? Isbn { get; private set; }
    public int? Edition { get; private set; }
    public string? Language { get; private set; }
    public int? TotalPages { get; private set; }
    public DateOnly? PublishedDate { get; private set; }
    public BookStatus Status { get; private set; } = BookStatus.Available;
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }

    public PublisherId? PublisherId { get; private set; }
    public List<ContributorId> ContributorIds { get; private set; } = [];
    public List<CategoryId> CategoryIds { get; private set; } = [];

    private Book() { }

    /// <summary>
    /// Creates a new book aggregate.
    /// </summary>
    public static Book Create(
        string title,
        IsbnNumber isbn,
        PublisherId publisherId,
        int totalCopies,
        string? description = null,
        int? edition = null,
        string? language = null,
        int? totalPages = null,
        DateOnly? publishedDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        if (totalCopies <= 0)
            throw new ArgumentException("Total copies must be greater than 0");

        var book = new Book
        {
            Id = BookId.New(),
            Title = title.Trim(),
            Isbn = isbn,
            PublisherId = publisherId,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies,
            Description = description?.Trim(),
            Edition = edition,
            Language = language?.Trim(),
            TotalPages = totalPages,
            PublishedDate = publishedDate,
            Status = BookStatus.Available
        };

        book.RaiseDomainEvent(new BookCreatedEvent(book.Id, book.Title, isbn.Value));
        return book;
    }

    /// <summary>
    /// Updates book information.
    /// </summary>
    public void Update(
        string title,
        string? description = null,
        int? edition = null,
        string? language = null,
        int? totalPages = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        Title = title.Trim();
        Description = description?.Trim();
        Edition = edition;
        Language = language?.Trim();
        TotalPages = totalPages;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookUpdatedEvent(Id, Title));
    }

    /// <summary>
    /// Adds a contributor to the book.
    /// </summary>
    public void AddContributor(ContributorId contributorId)
    {
        if (ContributorIds.Contains(contributorId))
            throw new InvalidOperationException("Contributor already assigned to this book");

        ContributorIds.Add(contributorId);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a contributor from the book.
    /// </summary>
    public void RemoveContributor(ContributorId contributorId)
    {
        if (!ContributorIds.Contains(contributorId))
            throw new InvalidOperationException("Contributor not assigned to this book");

        ContributorIds.Remove(contributorId);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a category to the book.
    /// </summary>
    public void AddCategory(CategoryId categoryId)
    {
        if (CategoryIds.Contains(categoryId))
            throw new InvalidOperationException("Category already assigned to this book");

        CategoryIds.Add(categoryId);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a category from the book.
    /// </summary>
    public void RemoveCategory(CategoryId categoryId)
    {
        if (!CategoryIds.Contains(categoryId))
            throw new InvalidOperationException("Category not assigned to this book");

        CategoryIds.Remove(categoryId);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates availability when copies are checked out.
    /// </summary>
    public void DecreaseAvailableCopies(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (AvailableCopies - quantity < 0)
            throw new InvalidOperationException("Not enough available copies");

        AvailableCopies -= quantity;
        UpdateAvailabilityStatus();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookAvailabilityChangedEvent(Id, AvailableCopies, TotalCopies));
    }

    /// <summary>
    /// Updates availability when copies are returned.
    /// </summary>
    public void IncreaseAvailableCopies(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (AvailableCopies + quantity > TotalCopies)
            throw new InvalidOperationException("Cannot exceed total copies");

        AvailableCopies += quantity;
        UpdateAvailabilityStatus();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookAvailabilityChangedEvent(Id, AvailableCopies, TotalCopies));
    }

    /// <summary>
    /// Marks book as lost.
    /// </summary>
    public void MarkAsLost(int lostCopies)
    {
        if (lostCopies <= 0)
            throw new ArgumentException("Lost copies must be greater than 0");

        if (lostCopies > TotalCopies)
            throw new InvalidOperationException("Lost copies cannot exceed total");

        Status = BookStatus.Lost;
        TotalCopies -= lostCopies;
        if (AvailableCopies > TotalCopies)
            AvailableCopies = TotalCopies;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks book as damaged.
    /// </summary>
    public void MarkAsDamaged(int damagedCopies)
    {
        if (damagedCopies <= 0)
            throw new ArgumentException("Damaged copies must be greater than 0");

        if (damagedCopies > TotalCopies)
            throw new InvalidOperationException("Damaged copies cannot exceed total");

        Status = BookStatus.Damaged;
        TotalCopies -= damagedCopies;
        if (AvailableCopies > TotalCopies)
            AvailableCopies = TotalCopies;

        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateAvailabilityStatus()
    {
        if (!IsActive)
        {
            Status = BookStatus.Inactive;
        }
        else if (AvailableCopies == 0)
        {
            Status = BookStatus.Unavailable;
        }
        else if (Status != BookStatus.Lost && Status != BookStatus.Damaged)
        {
            Status = BookStatus.Available;
        }
    }

    /// <summary>
    /// Deactivates the book (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();
        Status = BookStatus.Inactive;
        RaiseDomainEvent(new BookDeactivatedEvent(Id));
    }
}
