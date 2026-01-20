namespace Core.Domain;

/// <summary>
/// Interface that marks a type as a domain event.
/// Domain events represent something that happened in the domain and are important to the business.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The moment when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Unique ID for event tracking.
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// The moment when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Unique ID for event tracking.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();
}
