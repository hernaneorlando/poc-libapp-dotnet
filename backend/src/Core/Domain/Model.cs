namespace Core.Domain;

/// <summary>
/// Base class for entities in DDD.
/// Entities have unique identity and can change state over time.
/// </summary>
/// <typeparam name="TId">Type of the unique identifier of the entity (e.g., UserId, BookId)</typeparam>
public abstract class Model<TId> : IEquatable<Model<TId>>
    where TId : ValueObject
{
    /// <summary>
    /// Unique identifier of the entity (Value Object).
    /// </summary>
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Entity creation date in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update date in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the entity is active or logically deleted.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Domain events that have occurred on this entity.
    /// Should be cleared after persistence.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Returns a read-only copy of the unprocessed domain events.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Clears all recorded domain events.
    /// Should be called after successful persistence.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Registers a domain event that occurred on this entity.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Compares two entities by identity.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Model<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Compares two entities by identity.
    /// </summary>
    public bool Equals(Model<TId>? other) => Equals((object?)other);

    /// <summary>
    /// Returns the hash code based on identity.
    /// </summary>
    public override int GetHashCode() => Id?.GetHashCode() ?? throw new InvalidOperationException("Entity ID cannot be null");

    /// <summary>
    /// Equality operator for entities.
    /// </summary>
    public static bool operator ==(Model<TId> left, Model<TId> right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for entities.
    /// </summary>
    public static bool operator !=(Model<TId> left, Model<TId> right) => !(left == right);
}
