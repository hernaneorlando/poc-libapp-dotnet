namespace Domain.UserManagement.ValueObjects;

public class UserContact(string email)
{
    public string Email { get; set; } = email;
    public string? PhoneNumber { get; set; }
    public Address? Address { get; set; }
}