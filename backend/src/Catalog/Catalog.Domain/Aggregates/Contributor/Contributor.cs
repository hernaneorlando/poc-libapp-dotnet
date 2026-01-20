namespace Catalog.Domain.Aggregates.Contributor;

/// <summary>
/// Aggregate Root representing a Contributor (author, illustrator, translator, etc).
/// </summary>
public sealed class Contributor : AggregateRoot<ContributorId>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public string? Biography { get; private set; }

    private Contributor() { }

    /// <summary>
    /// Creates a new contributor aggregate.
    /// </summary>
    public static Contributor Create(
        string firstName,
        string lastName,
        DateOnly? dateOfBirth = null,
        string? biography = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty");

        var contributor = new Contributor
        {
            Id = ContributorId.New(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth,
            Biography = biography?.Trim()
        };

        contributor.RaiseDomainEvent(new ContributorCreatedEvent(
            contributor.Id,
            contributor.FirstName,
            contributor.LastName));

        return contributor;
    }

    /// <summary>
    /// Updates contributor information.
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        DateOnly? dateOfBirth = null,
        string? biography = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Biography = biography?.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ContributorUpdatedEvent(Id, FirstName, LastName));
    }

    /// <summary>
    /// Gets the full name of the contributor.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>
    /// Deactivates the contributor (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();
        RaiseDomainEvent(new ContributorDeactivatedEvent(Id));
    }
}
