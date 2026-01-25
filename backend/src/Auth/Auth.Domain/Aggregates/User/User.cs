namespace Auth.Domain.Aggregates.User;

using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;

/// <summary>
/// Aggregate Root representing a User in the authentication system.
/// Manages user profile, authentication, and role assignments.
/// 
/// Authorization Model:
/// - User has multiple Roles, each Role has a set of Permissions
/// - User can explicitly deny specific Permissions (exceptions to their roles)
/// - Permission evaluation: Deny (top priority) > Role Permissions (default)
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Username Username { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public UserContact Contact { get; set; } = null!;
    public UserType UserType { get; set; }
    public List<Role> Roles { get; private set; } = [];
    public List<Permission> DeniedPermissions { get; private set; } = [];
    public List<RefreshToken> RefreshTokens { get; private set; } = [];

    public User() { }

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Creates a new user account.
    /// If username is null, it will be auto-generated from firstName and lastName.
    /// If username is provided, it should have been pre-validated by IUsernameGeneratorService.
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="email">User's email address</param>
    /// <param name="userType">The type of user (Customer or Employee)</param>
    /// <param name="username">Pre-validated username (null to auto-generate from names)</param>
    /// <param name="phoneNumber">Optional phone number</param>
    public static User Create(
        string firstName,
        string lastName,
        string email,
        UserType userType,
        Username? username = null,
        string? phoneNumber = null)
    {
        Validate(firstName, lastName);

        var contact = UserContact.Create(email, phoneNumber);

        // If username not provided, generate from names
        var finalUsername = username ?? Username.GenerateFromName(firstName, lastName);

        var user = new User
        {
            Id = UserId.New(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Username = finalUsername,
            Contact = contact,
            UserType = userType
        };

        user.RaiseDomainEvent(new UserCreatedEvent(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Username.Value,
            user.Contact.Email));

        return user;
    }

    /// <summary>
    /// Updates the user profile information.
    /// </summary>
    public void Update(string firstName, string lastName)
    {
        Validate(firstName, lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserUpdatedEvent(Id, FirstName, LastName));
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    public void SetPasswordHash(string passwordHash)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(passwordHash, "Password hash cannot be empty");

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserPasswordChangedEvent(Id));
    }

    /// <summary>
    /// Assigns a role to the user.
    /// </summary>
    public void AssignRole(Role role)
    {
        ValidationException.ThrowIfNull(role);
        
        if (Roles.Any(r => r.Id == role.Id))
            throw new InvalidOperationException("Role already assigned to this user");

        Roles.Add(role);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RoleAssignedToUserEvent(Id, role.Id.ToString()));
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(RoleId roleId)
    {
        ValidationException.ThrowIfNull(roleId);
        
        var role = Roles.FirstOrDefault(r => r.Id == roleId) 
            ?? throw new InvalidOperationException("Role not assigned to this user");

        Roles.Remove(role);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RoleRemovedFromUserEvent(Id, roleId.ToString()));
    }

    /// <summary>
    /// Adds a refresh token to the user.
    /// </summary>
    public void AddRefreshToken(RefreshToken refreshToken)
    {
        ValidationException.ThrowIfNull(refreshToken, "Refresh token cannot be null");
        if (RefreshTokens.Any(rt => rt.Token == refreshToken.Token))
            throw new InvalidOperationException("Refresh token already exists for this user");
        
        RefreshTokens.Add(refreshToken);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    public void RevokeRefreshToken(string token)
    {
        var refreshToken = RefreshTokens.FirstOrDefault(rt => rt.Token == token) 
            ?? throw new InvalidOperationException("Refresh token not found");
        refreshToken.Revoke();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();

        // Revoke all active refresh tokens
        foreach (var token in RefreshTokens.Where(rt => rt.IsValid))
        {
            token.Revoke();
        }

        RaiseDomainEvent(new UserDeactivatedEvent(Id));
    }

    private static void Validate(string firstName, string lastName)
    {
            ValidationException.ThrowIfNullOrWhiteSpace(firstName, "First name cannot be empty");
            ValidationException.ThrowIfNullOrWhiteSpace(lastName, "Last name cannot be empty");
    }
}
