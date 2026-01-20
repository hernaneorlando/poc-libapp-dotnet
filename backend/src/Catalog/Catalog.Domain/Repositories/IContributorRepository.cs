namespace Catalog.Domain.Repositories;

using Catalog.Domain.Aggregates.Contributor;

/// <summary>
/// Repository interface for Contributor aggregate persistence.
/// Defines write operations for contributors.
/// </summary>
public interface IContributorRepository
{
    /// <summary>
    /// Adds a new contributor to the repository.
    /// </summary>
    Task AddAsync(Contributor contributor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a contributor by its ID.
    /// </summary>
    Task<Contributor?> GetByIdAsync(ContributorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing contributor.
    /// </summary>
    Task UpdateAsync(Contributor contributor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a contributor (soft delete via deactivation).
    /// </summary>
    Task DeleteAsync(ContributorId id, CancellationToken cancellationToken = default);
}
