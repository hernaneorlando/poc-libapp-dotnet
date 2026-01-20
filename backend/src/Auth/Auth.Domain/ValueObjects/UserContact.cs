namespace Auth.Domain.ValueObjects;

/// <summary>
/// Value Object representing user contact information.
/// </summary>
public sealed class UserContact : ValueObject
{
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DocumentIdentification { get; set; }
    public Address? Address { get; set; }

    public UserContact() { }

    /// <summary>
    /// Creates a new user contact.
    /// </summary>
    public static UserContact Create(string email, string? phoneNumber = null, string? DocumentIdentification = null, Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (!email.Contains("@"))
            throw new ArgumentException("Email must be valid");

        return new UserContact
        {
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber?.Trim(),
            DocumentIdentification = DocumentIdentification?.Trim(),
            Address = address
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber;
        yield return DocumentIdentification;
        yield return Address;
    }
}
