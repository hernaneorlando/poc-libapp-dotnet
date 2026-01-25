namespace Auth.Domain.Aggregates.Role;

using Auth.Domain.Aggregates.Permission;

/// <summary>
/// Aggregate Root representing a Role in the authorization system.
/// A Role is a collection of Permissions that can be assigned to Users.
/// </summary>
public sealed class Role : AggregateRoot<RoleId>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Permission> Permissions { get; private set; } = [];

    public Role() { }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(string name, string description)
    {
        Validate(name, description);        
            
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
        Validate(name, description);

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
        ValidationException.ThrowIfNull(permission, "Permission cannot be null");
        ValidationException.ThrowIfPredicate(Permissions.Contains(permission), "Permission already assigned to this role");

        Permissions.Add(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns permissions to the role.
    /// </summary>
    public void AssignPermission(IEnumerable<Permission> permissions)
    {
        foreach (var permission in permissions)
            AssignPermission(permission);
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    public void RemovePermission(Permission permission)
    {
        ValidationException.ThrowIfNull(permission, "Permission cannot be null");
        ValidationException.ThrowIfPredicate(!Permissions.Contains(permission), "Permission not assigned to this role");

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

    private static void Validate(string name, string description)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(name, "Name cannot be empty");
        ValidationException.ThrowIfNullOrWhiteSpace(description, "Description cannot be empty");
        ValidationException.ThrowIfPredicate(name.Length < 3, "Name must be at least 3 characters");
        ValidationException.ThrowIfPredicate(name.Length > 50, "Name must not exceed 50 characters");
        ValidationException.ThrowIfPredicate(description.Length < 10, "Description must be at least 10 characters");
        ValidationException.ThrowIfPredicate(description.Length > 256, "Description must not exceed 256 characters");
    }
}