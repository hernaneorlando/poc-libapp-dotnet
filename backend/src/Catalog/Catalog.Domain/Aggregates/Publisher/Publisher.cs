namespace Catalog.Domain.Aggregates.Publisher;

/// <summary>
/// Aggregate Root representing a Publisher.
/// </summary>
public sealed class Publisher : AggregateRoot<PublisherId>
{
    public string Name { get; private set; } = string.Empty;
    public DateOnly? FoundationDate { get; private set; }
    public PublisherContact? Contact { get; private set; }

    private Publisher() { }

    /// <summary>
    /// Creates a new publisher aggregate.
    /// </summary>
    public static Publisher Create(
        string name,
        DateOnly? foundationDate = null,
        PublisherContact? contact = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Publisher name cannot be empty");

        var publisher = new Publisher
        {
            Id = PublisherId.New(),
            Name = name.Trim(),
            FoundationDate = foundationDate,
            Contact = contact
        };

        publisher.RaiseDomainEvent(new PublisherCreatedEvent(publisher.Id, publisher.Name));

        return publisher;
    }

    /// <summary>
    /// Updates publisher information.
    /// </summary>
    public void Update(
        string name,
        DateOnly? foundationDate = null,
        PublisherContact? contact = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Publisher name cannot be empty");

        Name = name.Trim();
        FoundationDate = foundationDate;
        Contact = contact;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PublisherUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Updates contact information.
    /// </summary>
    public void UpdateContact(PublisherContact contact)
    {
        Contact = contact;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the publisher (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();
        RaiseDomainEvent(new PublisherDeactivatedEvent(Id));
    }
}
