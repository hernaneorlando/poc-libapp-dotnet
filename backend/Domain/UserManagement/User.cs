using Domain.UserManagement.ValueObjects;
using Domain.Common;
using Domain.Common.Interfaces;

namespace Domain.UserManagement;

public class User : DocumentDbModel<User>, IAggregateRoot
{
    public string Username { get; private set; }
    public string? PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? DocumentIdentification { get; set; }
    public UserContact Contact { get; set; }
    public ICollection<Role> Roles { get; set; } = [];

    public User(string id)
        : this(string.Empty, string.Empty, string.Empty, null!)
    {
        Id = id;
    }

    public User(string firstName, string lastName, string email, Role role)
        : this(firstName, lastName, new UserContact(email), role) { }

    public User(string firstName, string lastName, UserContact contact, Role role)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Username = $"{FirstName.ToLower()}.{LastName.ToLower()}";
        Contact = contact;
        Roles.Add(role);
    }

    public void AssignRole(Role role)
    {
        if (!Roles.Contains(role))
            Roles.Add(role);
    }

    public bool HasPermission(Permission permission) =>
        Roles.Any(r => r.Permissions.Any(p => p == permission));
}