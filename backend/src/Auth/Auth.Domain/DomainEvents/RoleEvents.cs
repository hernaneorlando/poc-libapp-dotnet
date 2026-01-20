namespace Auth.Domain.DomainEvents;

using Auth.Domain.Aggregates.Role;

/// <summary>
/// Domain event raised when a role is created.
/// </summary>
public sealed class RoleCreatedEvent(RoleId roleId, string name, string description) : DomainEvent
{
    public RoleId RoleId { get; } = roleId;
    public string Name { get; } = name;
    public string Description { get; } = description;
}

/// <summary>
/// Domain event raised when a role is updated.
/// </summary>
public sealed class RoleUpdatedEvent(RoleId roleId, string name, string description) : DomainEvent
{
    public RoleId RoleId { get; } = roleId;
    public string Name { get; } = name;
    public string Description { get; } = description;
}

/// <summary>
/// Domain event raised when a role is deactivated (soft delete).
/// </summary>
public sealed class RoleDeactivatedEvent(RoleId roleId) : DomainEvent
{
    public RoleId RoleId { get; } = roleId;
}
