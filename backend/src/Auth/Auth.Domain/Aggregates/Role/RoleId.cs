namespace Auth.Domain.Aggregates.Role;

/// <summary>
/// Value Object representing the unique identifier for a Role aggregate.
/// </summary>
public sealed class RoleId : ValueObject
{
    public Guid Value { get; private set; }

    private RoleId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new RoleId from a GUID.
    /// </summary>
    public static RoleId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty");

        return new RoleId(value);
    }

    /// <summary>
    /// Generates a new unique RoleId.
    /// </summary>
    public static RoleId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
