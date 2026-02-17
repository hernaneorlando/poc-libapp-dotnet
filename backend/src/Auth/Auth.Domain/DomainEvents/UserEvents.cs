namespace Auth.Domain.DomainEvents;

using Auth.Domain.Aggregates.User;

/// <summary>
/// Domain event raised when a user is created.
/// </summary>
public sealed class UserCreatedEvent(UserId userId, string firstName, string lastName, string username, string email) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Username { get; } = username;
    public string Email { get; } = email;
}

/// <summary>
/// Domain event raised when a user's profile is updated.
/// </summary>
public sealed class UserUpdatedEvent(UserId userId, string firstName, string lastName) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
}

/// <summary>
/// Domain event raised when a user's password is changed.
/// </summary>
public sealed class UserPasswordChangedEvent(UserId userId) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public DateTime ChangedAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a user is deactivated (soft delete).
/// </summary>
public sealed class UserDeactivatedEvent(UserId userId) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public DateTime DeactivatedAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a role is assigned to a user.
/// </summary>
public sealed class RoleAssignedToUserEvent(UserId userId, string roleName) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public string RoleName { get; } = roleName;
}

/// <summary>
/// Domain event raised when a role is removed from a user.
/// </summary>
public sealed class RoleRemovedFromUserEvent(UserId userId, string roleName) : DomainEvent
{
    public UserId UserId { get; } = userId;
    public string RoleName { get; } = roleName;
}

/// <summary>
/// Domain event raised when a user explicitly logs out.
/// Triggers revocation of ALL user's refresh tokens across all devices.
/// </summary>
public sealed class UserLoggedOutEvent(UserId userId) : DomainEvent, MediatR.INotification
{
    public UserId UserId { get; } = userId;
}
