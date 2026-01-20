namespace Auth.Domain.Aggregates.User;

/// <summary>
/// Value Object representing the unique identifier for a User aggregate.
/// </summary>
public sealed class UserId : ValueObject
{
    public Guid Value { get; private set; }

    private UserId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new UserId from a GUID.
    /// </summary>
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");

        return new UserId(value);
    }

    /// <summary>
    /// Generates a new unique UserId.
    /// </summary>
    public static UserId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
