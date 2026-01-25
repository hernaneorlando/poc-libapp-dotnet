using System.Linq.Expressions;

namespace Core.Infrastructure;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Predicate { get; }
}
