namespace Catalog.Domain.Aggregates.Publisher;

/// <summary>
/// Value Object representing the unique identifier of a Publisher aggregate.
/// </summary>
public sealed class PublisherId : ValueObject
{
    public Guid Value { get; }

    private PublisherId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PublisherId cannot be empty");
        Value = value;
    }

    public static PublisherId New() => new(Guid.NewGuid());

    public static PublisherId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
