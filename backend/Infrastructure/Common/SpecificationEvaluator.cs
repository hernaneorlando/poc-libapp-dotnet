using Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Common;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)
        where T : class
    {
        var query = inputQuery;

        // Where
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // String includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        return query;
    }
}