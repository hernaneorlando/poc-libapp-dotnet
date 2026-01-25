namespace Core.Domain;

/// <summary>
/// Base class for Aggregate Roots in DDD.
/// An Aggregate Root is the root of an aggregate and is responsible for maintaining its consistency.
/// Aggregates are transactional and persistence boundaries.
/// </summary>
/// <typeparam name="TId">Type of the unique identifier of the aggregate (e.g., BookId, UserId)</typeparam>
public abstract class AggregateRoot<TId> : Model<TId>
    where TId : ValueObject
{
    /// <summary>
    /// Optimistic version of the aggregate for concurrency control.
    /// Increments with each successful modification.
    /// </summary>
    public int Version { get; set; } = 1;

    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Increments the version of the aggregate.
    /// Should be called when persisting changes.
    /// </summary>
    public void IncrementVersion() => Version++;

    /// <summary>
    /// Deactivates the aggregate (soft delete).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AggregateDeactivatedEvent(Id!.GetType().Name, Id.GetHashCode()));
    }

    /// <summary>
    /// Reactivates the aggregate.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Domain event that represents the deactivation of an aggregate.
/// </summary>
public sealed class AggregateDeactivatedEvent(string aggregateTypeName, int aggregateIdHash) : DomainEvent
{
    /// <summary>
    /// Name of the aggregate type that was deactivated.
    /// </summary>
    public string AggregateTypeName { get; } = aggregateTypeName;

    /// <summary>
    /// Hash code of the aggregate identifier.
    /// </summary>
    public int AggregateIdHash { get; } = aggregateIdHash;
}
