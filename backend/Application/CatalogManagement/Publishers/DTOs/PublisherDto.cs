using Application.CatalogManagement.Books.DTOs;
using Application.Common;
using Application.SeedWork.BaseDTO;
using Domain.CatalogManagement;
using Domain.CatalogManagement.ValueObjects;

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

        publisherDto.ConvertModelBaseProperties(publisher);
        return publisherDto;
    }

    public static implicit operator Publisher(PublisherDto publisherDto)
    {
        var publisher = new Publisher
        {
            Name = publisherDto.Name,
            FoundationDate = publisherDto.FoundationDate,
            Contact = publisherDto.Contact != null ? (Contact)publisherDto.Contact : null,
            Books = [.. publisherDto.Books.Select(b => (Book)b)]
        };

        publisher.ConvertDtoBaseProperties(publisherDto);
        return publisher;
    }
}