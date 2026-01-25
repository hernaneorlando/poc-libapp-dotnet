using System.Linq.Expressions;
using Core.Infrastructure.Enums;

namespace Core.Infrastructure;

public static class QueryBuilder
{
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IEnumerable<FilterCriteria> filters)
    {
        foreach (var filter in filters)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, filter.Field);

            Expression condition = filter.Values.Count != 0
                ? CreateConditionFromValues(property, filter.Values, filter.Operator)
                : CreateConditionFromUniqueValue(property, filter.Value, filter.Operator);

            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    private static Expression CreateConditionFromUniqueValue(MemberExpression property, string filterValue, FilterOperator filterOperator)
    {
        object? typedValue = GetTypeValue(property, filterValue);
        var value = Expression.Constant(Convert.ChangeType(typedValue, property.Type));

        Expression condition = filterOperator switch
        {
            FilterOperator.Contains => Expression.Call(property,
                typeof(string).GetMethod("Contains", [typeof(string)])!,
                value),
            FilterOperator.StartsWith => Expression.Call(property,
                typeof(string).GetMethod("StartsWith", [typeof(string)])!,
                value),
            FilterOperator.EndsWith => Expression.Call(property,
                typeof(string).GetMethod("EndsWith", [typeof(string)])!,
                value),
            FilterOperator.GreaterThan => Expression.GreaterThan(property, value),
            FilterOperator.LessThan => Expression.LessThan(property, value),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, value),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, value),
            _ => Expression.Equal(property, value)
        };

        return condition;
    }

    private static Expression CreateConditionFromValues(MemberExpression property, ICollection<string> filterValues, FilterOperator filterOperator)
    {
        Expression body = null!;
        foreach (var value in filterValues)
        {
            object? typedValue = GetTypeValue(property, value);
            var constant = Expression.Constant(Convert.ChangeType(typedValue, property.Type));
            var equals = Expression.Equal(property, constant);
            if (body == null)
                body = equals;
            else
                body = Expression.Or(body, equals);
        }

        return body;
    }

    private static object? GetTypeValue(MemberExpression property, string value)
    {
        return property.Type switch
        {
            _ when property.Type == typeof(Guid) => Guid.TryParse(value, out Guid id) ? id : Guid.Empty,
            _ when property.Type.IsEnum => Enum.TryParse(property.Type, value, out object? enumValue) ? enumValue : null,
            _ => value
        };
    }

    public static IQueryable<T> ApplyOrder<T>(
        this IQueryable<T> query,
        string orderByString)
    {
        if (string.IsNullOrEmpty(orderByString))
            return query;

        var orderCriteria = orderByString
            .Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x));

        var orderMethodName = "OrderBy";
        foreach (var criteria in orderCriteria)
        {
            var parts = criteria.Split(' ');
            var propertyName = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var parameter = Expression.Parameter(typeof(T), "x");

            var property = propertyName.Split('.')
                .Aggregate((Expression)parameter, Expression.Property);

            var lambda = Expression.Lambda(property, parameter);

            var method = typeof(Queryable)
                .GetMethods()
                .Where(m => m.Name == (descending ? orderMethodName + "Descending" : orderMethodName))
                .Single(m => m.GetParameters().Length == 2);

            var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

            query = (IQueryable<T>)genericMethod.Invoke(null, [query, lambda])!;

            // Switch to ThenBy for subsequent order criteria
            orderMethodName = "ThenBy";
        }

        return query;
    }
}