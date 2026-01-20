namespace Auth.Domain.ValueObjects;

/// <summary>
/// Value Object representing an address.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Complement {get; set; } = string.Empty;
    public string? ZipCode { get; set; }

    public Address() { }

    /// <summary>
    /// Creates a new address.
    /// </summary>
    public static Address Create(string street, string city, string state, string country, string? complement = null, string? zipCode = null)
    {
        if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city) || 
            string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Address fields cannot be empty");

        return new Address
        {
            Street = street.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            Country = country.Trim(),
            Complement = complement?.Trim(),
            ZipCode = zipCode?.Trim()
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return Complement;
        yield return ZipCode;
    }
}
