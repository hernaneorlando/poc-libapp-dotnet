using Domain.SeedWork.Enums;

namespace Domain.UserManagement.ValueObjects;

public class Address
{
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public CountryEnum? Country { get; set; }
}