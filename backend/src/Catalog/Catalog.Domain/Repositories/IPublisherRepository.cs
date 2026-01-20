namespace Catalog.Domain.Repositories;

using Catalog.Domain.Aggregates.Publisher;

/// <summary>
/// Repository interface for Publisher aggregate persistence.
/// Defines write operations for publishers.
/// </summary>
public interface IPublisherRepository
{
    /// <summary>
    /// Adds a new publisher to the repository.
    /// </summary>
    Task AddAsync(Publisher publisher, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a publisher by its ID.
    /// </summary>
    Task<Publisher?> GetByIdAsync(PublisherId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a publisher by its name.
    /// </summary>
    Task<Publisher?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing publisher.
    /// </summary>
    Task UpdateAsync(Publisher publisher, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a publisher (soft delete via deactivation).
    /// </summary>
    Task DeleteAsync(PublisherId id, CancellationToken cancellationToken = default);
}
