using Domain.SeedWork.Enums;
using Domain.UserManagement.ValueObjects;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class AddressValueObject
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public CountryEnum? Country { get; set; }

    public static implicit operator Address(AddressValueObject addressValueObject)
    {
        return new Address
        {
            AddressLine1 = addressValueObject.AddressLine1,
            AddressLine2 = addressValueObject.AddressLine2,
            PostalCode = addressValueObject.PostalCode,
            City = addressValueObject.City,
            Region = addressValueObject.Region,
            Country = addressValueObject.Country
        };
    }

    public static implicit operator AddressValueObject(Address address)
    {
        return new AddressValueObject
        {
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            PostalCode = address.PostalCode,
            City = address.City,
            Region = address.Region,
            Country = address.Country,
        };
    }
}