using Domain.UserManagement.ValueObjects;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class ContactValueObject
{
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AddressValueObject? Address { get; set; }

    public static implicit operator UserContact(ContactValueObject contact)
    {
        return new UserContact(contact.Email)
        {
            PhoneNumber = contact.PhoneNumber,
            Address = contact.Address != null ? (Address)contact.Address : null
        };
    }

    public static implicit operator ContactValueObject(UserContact contact)
    {
        return new ContactValueObject
        {
            Email = contact.Email,
            PhoneNumber = contact.PhoneNumber,
            Address = contact.Address != null ? (AddressValueObject)contact.Address : null
        };
    }
}