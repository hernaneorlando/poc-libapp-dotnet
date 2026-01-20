namespace Catalog.Domain.DomainEvents;

using Catalog.Domain.Aggregates.Book;

/// <summary>
/// Domain event raised when a book is created.
/// </summary>
public sealed class BookCreatedEvent : DomainEvent
{
    public BookId BookId { get; }
    public string Title { get; }
    public string Isbn { get; }

    public BookCreatedEvent(BookId bookId, string title, string isbn)
    {
        BookId = bookId;
        Title = title;
        Isbn = isbn;
    }
}

/// <summary>
/// Domain event raised when a book is updated.
/// </summary>
public sealed class BookUpdatedEvent : DomainEvent
{
    public BookId BookId { get; }
    public string Title { get; }

    public BookUpdatedEvent(BookId bookId, string title)
    {
        BookId = bookId;
        Title = title;
    }
}

/// <summary>
/// Domain event raised when a book is deactivated.
/// </summary>
public sealed class BookDeactivatedEvent : DomainEvent
{
    public BookId BookId { get; }

    public BookDeactivatedEvent(BookId bookId)
    {
        BookId = bookId;
    }
}

/// <summary>
/// Domain event raised when a book availability changes.
/// </summary>
public sealed class BookAvailabilityChangedEvent : DomainEvent
{
    public BookId BookId { get; }
    public int AvailableCopies { get; }
    public int TotalCopies { get; }

    public BookAvailabilityChangedEvent(BookId bookId, int availableCopies, int totalCopies)
    {
        BookId = bookId;
        AvailableCopies = availableCopies;
        TotalCopies = totalCopies;
    }
}
