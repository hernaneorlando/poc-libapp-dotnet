namespace Core.Domain;

/// <summary>
/// Base class for Value Objects in DDD.
/// Value Objects are immutable and do not have unique identity.
/// Two Value Objects with the same values are considered equal.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the components that define the equality of this Value Object.
    /// Implementations should return the fields/properties that define identity.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Compares this Value Object with another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns the hash code based on equality components.
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Compares this Value Object with another Value Object.
    /// </summary>
    public bool Equals(ValueObject? other) => Equals((object?)other);

    /// <summary>
    /// Equality operator for Value Objects.
    /// </summary>
    public static bool operator ==(ValueObject left, ValueObject right) => Equals(left, right);

    /// <summary>
    /// Inequality operator for Value Objects.
    /// </summary>
    public static bool operator !=(ValueObject left, ValueObject right) => !Equals(left, right);
}
