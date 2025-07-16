using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Publishers.DTOs;
using Domain.CatalogManagement;
using Domain.CatalogManagement.ValueObjects;
using Infrastructure.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class PublisherEntity : RelationalDbBaseBaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly? FoundationDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public IList<BookEntity> Books { get; set; } = [];

    public static implicit operator PublisherDto(PublisherEntity entity)
    {
        var dto = new PublisherDto(entity.Name)
        {
            Id = entity.ExternalId,
            FoundationDate = entity.FoundationDate,
            Contact = new ContactDto
            {
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Website = entity.Website,
            },
            Books = [.. entity.Books.Select(b => (BookDto)b)],
        };

        dto.ConvertBaseProperties<Publisher>(entity);
        return dto;
    }

    public static implicit operator Publisher(PublisherEntity entity)
    {
        var model = new Publisher
        {
            ExternalId = entity.ExternalId,
            Name = entity.Name,
            FoundationDate = entity.FoundationDate,
            Contact = new PublisherContact()
            {
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Website = entity.Website,
            },
            Books = [.. entity.Books.Select(b => (Book)b)],
        };

        model.ConvertEntityBaseProperties(entity);
        return model;
    }
    
    public static implicit operator PublisherEntity(Publisher publisher)
    {
        var entity = new PublisherEntity()
        {
            ExternalId = publisher.ExternalId,
            Name = publisher.Name,
            FoundationDate = publisher.FoundationDate,
            PhoneNumber = publisher.Contact?.PhoneNumber,
            Email = publisher.Contact?.Email,
            Website = publisher.Contact?.Website,
            Books = [.. publisher.Books.Select(b => (BookEntity)b)],
        };

        entity.ConvertModelBaseProperties(publisher);
        return entity;
    }
}