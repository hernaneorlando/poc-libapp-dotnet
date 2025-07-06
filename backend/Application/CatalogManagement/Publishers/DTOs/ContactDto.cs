using Domain.CatalogManagement.ValueObjects;

namespace Application.CatalogManagement.Publishers.DTOs;

public record ContactDto
{
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    public static implicit operator ContactDto(Contact contact) => new()
    {
        PhoneNumber = contact.PhoneNumber,
        Email = contact.Email,
        Website = contact.Website
    };
}
