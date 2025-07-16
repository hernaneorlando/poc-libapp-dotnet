using Application.CatalogManagement.Books.DTOs;
using Application.Common.BaseDTO;
using Domain.CatalogManagement;

namespace Application.CatalogManagement.Publishers.DTOs;

public record PublisherDto(string Name) : BaseDto
{
    public DateOnly? FoundationDate { get; set; }
    public ContactDto? Contact { get; set; }
    public IList<BookDto> Books { get; set; } = [];

    public static implicit operator PublisherDto(Publisher publisher)
    {
        var publisherDto = new PublisherDto(publisher.Name)
        {
            FoundationDate = publisher.FoundationDate,
            Contact = publisher.Contact != null ? (ContactDto)publisher.Contact : null,
            Books = [.. publisher.Books.Select(b => (BookDto)b)]
        };

        publisherDto.ConvertBaseProperties(publisher);
        return publisherDto;
    }
}