using Core.Infrastructure.Enums;

namespace Core.Infrastructure;

public class FilterCriteria
{
    public string Field { get; } = string.Empty;
    public string Value { get; } = string.Empty;
    public ICollection<string> Values { get; } = [];
    public FilterOperator Operator { get; }

    public FilterCriteria(string field, string value, FilterOperator filterOperator)
    {
        Field = field;
        Value = value;
        Operator = filterOperator;
        Values = [];
    }

    public FilterCriteria(string field, string[] values, FilterOperator filterOperator)
    {
        Field = field;
        Value = string.Empty;
        Operator = filterOperator;
        Values = values;
    }

    public static FilterCriteria Parse(string field, string value)
    {
        if (value.StartsWith('*') && value.EndsWith('*'))
            return new FilterCriteria(field, value.Trim('*'), FilterOperator.Contains);

        if (value.StartsWith('*'))
            return new FilterCriteria(field, value.TrimStart('*'), FilterOperator.EndsWith);

        if (value.EndsWith('*'))
            return new FilterCriteria(field, value.TrimEnd('*'), FilterOperator.StartsWith);

        return new FilterCriteria(field, value, FilterOperator.Equals);
    }

    public static FilterCriteria Parse(string field, IEnumerable<string> values)
    {
        return new FilterCriteria(field, [.. values], FilterOperator.Or);
    }
}
