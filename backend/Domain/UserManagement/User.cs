using Domain.UserManagement.ValueObjects;
using Domain.SeedWork;

namespace Domain.UserManagement;

public class User : DocumentDbModel
{
    public string Username { get; private set; }
    public string? PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? DocumentIdentification { get; set; }
    public UserContact Contact { get; set; }
    public Role Role { get; set; }

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
        Role = role;
    }
}