namespace Catalog.Domain.Aggregates.Book;

/// <summary>
/// Value Object representing the unique identifier of a Book aggregate.
/// </summary>
public sealed class BookId : ValueObject
{
    public Guid Value { get; }

    private BookId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BookId cannot be empty");
        Value = value;
    }

    public static BookId New() => new(Guid.NewGuid());

    public static BookId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
