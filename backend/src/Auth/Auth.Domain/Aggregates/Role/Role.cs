namespace Auth.Domain.Aggregates.Role;

using Auth.Domain.Aggregates.Permission;
using Core.Validation;

/// <summary>
/// Aggregate Root representing a Role in the authorization system.
/// A Role is a collection of Permissions that can be assigned to Users.
/// </summary>
public sealed class Role : AggregateRoot<RoleId>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = [];

    public Role() { }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Description cannot be empty");

        if (name.Length < 3)
            throw new ValidationException("Name must be at least 3 characters");

        if (name.Length > 50)
            throw new ValidationException("Name must not exceed 50 characters");

        if (description.Length < 10)
            throw new ValidationException("Description must be at least 10 characters");

        if (description.Length > 500)
            throw new ValidationException("Description must not exceed 500 characters");
            
        var role = new Role
        {
            Id = RoleId.New(),
            Name = name.Trim(),
            Description = description.Trim()
        };

        role.RaiseDomainEvent(new RoleCreatedEvent(role.Id, role.Name, role.Description));
        return role;
    }

    /// <summary>
    /// Updates the role information.
    /// </summary>
    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Description cannot be empty");

        Name = name.Trim();
        Description = description.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RoleUpdatedEvent(Id, Name, Description));
    }

    /// <summary>
    /// Assigns a permission to the role.
    /// </summary>
    public void AssignPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        
        if (Permissions.Contains(permission))
            throw new ValidationException("Permission already assigned to this role");

        Permissions.Add(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    public void RemovePermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        
        if (!Permissions.Contains(permission))
            throw new ValidationException("Permission not assigned to this role");

        Permissions.Remove(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the role (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();
        RaiseDomainEvent(new RoleDeactivatedEvent(Id));
    }
}
