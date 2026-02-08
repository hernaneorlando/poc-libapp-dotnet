using System.Linq.Expressions;

namespace Core.Infrastructure;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Predicate { get; }

    IEnumerable<SpecificationInclude<T>> Includes { get; }
}

public sealed record SpecificationInclude<T>
{
    public Expression<Func<T, object>> IncludeExpression { get; init; }
    public Expression<Func<object, object>>? ThenIncludeExpression { get; set; }

    public SpecificationInclude(Expression<Func<T, object>> includeExpression)
    {
        IncludeExpression = includeExpression;
    }
}