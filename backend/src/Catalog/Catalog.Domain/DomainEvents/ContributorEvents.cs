namespace Catalog.Domain.DomainEvents;

using Catalog.Domain.Aggregates.Contributor;

/// <summary>
/// Domain event raised when a contributor is created.
/// </summary>
public sealed class ContributorCreatedEvent : DomainEvent
{
    public ContributorId ContributorId { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public ContributorCreatedEvent(ContributorId contributorId, string firstName, string lastName)
    {
        ContributorId = contributorId;
        FirstName = firstName;
        LastName = lastName;
    }
}

/// <summary>
/// Domain event raised when a contributor is updated.
/// </summary>
public sealed class ContributorUpdatedEvent : DomainEvent
{
    public ContributorId ContributorId { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public ContributorUpdatedEvent(ContributorId contributorId, string firstName, string lastName)
    {
        ContributorId = contributorId;
        FirstName = firstName;
        LastName = lastName;
    }
}

/// <summary>
/// Domain event raised when a contributor is deactivated.
/// </summary>
public sealed class ContributorDeactivatedEvent : DomainEvent
{
    public ContributorId ContributorId { get; }

    public ContributorDeactivatedEvent(ContributorId contributorId)
    {
        ContributorId = contributorId;
    }
}
