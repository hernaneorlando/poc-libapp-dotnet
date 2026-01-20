namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object representing publisher contact information.
/// </summary>
public sealed class PublisherContact : ValueObject
{
    public string Email { get; }
    public string? Phone { get; }
    public string? Website { get; }

    private PublisherContact(string email, string? phone, string? website)
    {
        Email = email;
        Phone = phone;
        Website = website;
    }

    public static PublisherContact Create(string email, string? phone = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email format");

        return new PublisherContact(email.Trim(), phone?.Trim(), website?.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return Website;
    }

    public override string ToString() => Email;
}
