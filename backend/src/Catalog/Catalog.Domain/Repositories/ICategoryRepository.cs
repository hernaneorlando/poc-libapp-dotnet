namespace Catalog.Domain.Repositories;

using Catalog.Domain.Aggregates.Category;

/// <summary>
/// Repository interface for Category aggregate persistence.
/// Defines write operations for categories.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Adds a new category to the repository.
    /// </summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its ID.
    /// </summary>
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its name.
    /// </summary>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category (soft delete via deactivation).
    /// </summary>
    Task DeleteAsync(CategoryId id, CancellationToken cancellationToken = default);
}
