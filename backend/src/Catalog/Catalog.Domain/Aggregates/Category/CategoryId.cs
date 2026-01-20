namespace Catalog.Domain.Aggregates.Category;

/// <summary>
/// Value Object representing the unique identifier of a Category aggregate.
/// </summary>
public sealed class CategoryId : ValueObject
{
    public Guid Value { get; }

    private CategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CategoryId cannot be empty");
        Value = value;
    }

    public static CategoryId New() => new(Guid.NewGuid());

    public static CategoryId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
