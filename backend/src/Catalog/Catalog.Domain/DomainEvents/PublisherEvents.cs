namespace Catalog.Domain.DomainEvents;

using Catalog.Domain.Aggregates.Publisher;

/// <summary>
/// Domain event raised when a publisher is created.
/// </summary>
public sealed class PublisherCreatedEvent : DomainEvent
{
    public PublisherId PublisherId { get; }
    public string Name { get; }

    public PublisherCreatedEvent(PublisherId publisherId, string name)
    {
        PublisherId = publisherId;
        Name = name;
    }
}

/// <summary>
/// Domain event raised when a publisher is updated.
/// </summary>
public sealed class PublisherUpdatedEvent : DomainEvent
{
    public PublisherId PublisherId { get; }
    public string Name { get; }

    public PublisherUpdatedEvent(PublisherId publisherId, string name)
    {
        PublisherId = publisherId;
        Name = name;
    }
}

/// <summary>
/// Domain event raised when a publisher is deactivated.
/// </summary>
public sealed class PublisherDeactivatedEvent : DomainEvent
{
    public PublisherId PublisherId { get; }

    public PublisherDeactivatedEvent(PublisherId publisherId)
    {
        PublisherId = publisherId;
    }
}
