using Core.API;

namespace Core.Infrastructure;

/// <summary>
/// Repository interface for entities pagination, sorting, and filtering operations
/// </summary>
public interface IBaseRepository<TModel, TEntity>
    where TModel : class
    where TEntity : class
{
    /// <summary>
    /// Retrieves the list of entities paginated, sorted and filtered
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entities paginated, sorted and filtered if found</returns>
    Task<PaginatedList<TModel>> GetAllAsync<TResponse>(BasePagedQuery<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : class;

    /// <summary>
    /// Retrieves an entityby a pre-defined constraint
    /// </summary>
    /// <param name="specification">The constraint to find the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TModel?> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}
