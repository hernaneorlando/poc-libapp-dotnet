namespace Catalog.Domain.Aggregates.Contributor;

/// <summary>
/// Value Object representing the unique identifier of a Contributor aggregate.
/// </summary>
public sealed class ContributorId : ValueObject
{
    public Guid Value { get; }

    private ContributorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContributorId cannot be empty");
        Value = value;
    }

    public static ContributorId New() => new(Guid.NewGuid());

    public static ContributorId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
