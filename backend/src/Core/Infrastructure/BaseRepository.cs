using System.Linq.Expressions;
using Core.API;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure;

/// <summary>
/// Implementation of IPaginatedRepository using Entity Framework Core
/// </summary>
public abstract class BaseRepository<TModel, TId, TEntity>(DbContext context) : IBaseRepository<TModel, TEntity>
    where TModel : Model<TId>
    where TId : ValueObject
    where TEntity : Entity
{
    /// <summary>
    /// Retrieves the list of entities paginated, sorted and filtered
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entities paginated, sorted and filtered if found</returns>
    public async Task<PaginatedList<TModel>> GetAllAsync<TResponse>(BasePagedQuery<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var query = context.Set<TEntity>()
            .AsQueryable()
            .Where(e => e.IsActive)
            .ApplyFilters(request.Filters);

        if (!string.IsNullOrEmpty(request.OrderBy))
            query = query.ApplyOrder(request.OrderBy);

        var total = await query.CountAsync(cancellationToken);
        var resultEntities = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var items = resultEntities.Select(MapToDomain);
        return new PaginatedList<TModel>(items!, total, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// Retrieves an entityby a pre-defined constraint
    /// </summary>
    /// <param name="specification">The constraint to find the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, null otherwise</returns>
    public async Task<TModel?> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification?.Predicate is null)
            throw new ArgumentNullException(nameof(specification), "Specification must not be null and contain a valid predicate.");

        var query = context.Set<TEntity>().AsNoTracking();

        if (specification.Includes.Any())
        {
            foreach (var include in specification.Includes)
            {
                var includedQuery = query.Include(include.IncludeExpression);
                if (include.ThenIncludeExpression is not null)
                {
                    query = includedQuery.ThenInclude(include.ThenIncludeExpression);
                }
                else
                {
                    query = includedQuery;
                }
            }
        }

        var result = await query.FirstOrDefaultAsync(specification?.Predicate!, cancellationToken);

        return result is null ? null : MapToDomain(result!);
    }

    protected abstract TModel MapToDomain(TEntity entity);
}
