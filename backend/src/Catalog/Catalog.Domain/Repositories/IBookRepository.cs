namespace Catalog.Domain.Repositories;

using Catalog.Domain.Aggregates.Book;

/// <summary>
/// Repository interface for Book aggregate persistence.
/// Defines write operations for books.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Adds a new book to the repository.
    /// </summary>
    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its ID.
    /// </summary>
    Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its ISBN.
    /// </summary>
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    Task UpdateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a book (soft delete via deactivation).
    /// </summary>
    Task DeleteAsync(BookId id, CancellationToken cancellationToken = default);
}
