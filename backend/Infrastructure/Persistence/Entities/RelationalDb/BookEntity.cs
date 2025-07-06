using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Publishers.DTOs;
using Application.Common;
using Domain.CatalogManagement;
using Domain.CatalogManagement.Enums;
using Domain.CatalogManagement.ValueObjects;
using Infrastructure.Common;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class BookEntity : RelationalDbBaseBaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Description { get; set; }
    public int? Edition { get; set; }
    public string? Language { get; set; }
    public int? TotalPages { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public BookStatusEnum Status { get; set; } = BookStatusEnum.Available;

    public CategoryEntity? Category { get; set; }
    public long? CategoryId { get; set; }
    public PublisherEntity Publisher { get; set; } = new();
    public long? PublisherId { get; set; }
    public IList<BookContributorEntity> Contributors { get; set; } = [];
    public IList<BookCheckoutEntity> Checkouts { get; set; } = [];

    public static implicit operator BookDto(BookEntity entity)
    {
        var dto = new BookDto(entity.Title, (PublisherDto)entity.Publisher, entity.Status)
        {
            Id = entity.ExternalId,
            ISBN = string.IsNullOrEmpty(entity.ISBN) ? string.Empty : Isbn.Create(entity.ISBN),
            Description = entity.Description,
            Edition = entity.Edition,
            Language = entity.Language,
            TotalPages = entity.TotalPages,
            PublishedDate = entity.PublishedDate,
            Category = entity.Category != null ? (CategoryDto)entity.Category : null,
        };

        dto.ConvertModelBaseProperties(entity);
        return dto;
    }

    public static implicit operator Book(BookEntity entity)
    {
        var model = new Book()
        {
            Title = entity.Title,
            ISBN = entity.ISBN != null ? Isbn.Create(entity.ISBN) : null,
            Description = entity.Description,
            Edition = entity.Edition,
            Language = entity.Language,
            TotalPages = entity.TotalPages,
            PublishedDate = entity.PublishedDate,
            Status = entity.Status,
            Category = entity.Category != null ? (Category)entity.Category : null,
            Publisher = entity.Publisher,
        };

        model.ConvertEntityBaseProperties(entity);
        return model;
    }

    public static implicit operator BookEntity(Book book)
    {
        var entity = new BookEntity()
        {
            Title = book.Title,
            ISBN = book.ISBN?.ToString(),
            Description = book.Description,
            Edition = book.Edition,
            Language = book.Language,
            TotalPages = book.TotalPages,
            PublishedDate = book.PublishedDate,
            Status = book.Status,
            Category = book.Category != null ? (CategoryEntity)book.Category : null,
            Publisher = book.Publisher,
        };

        entity.ConvertModelBaseProperties(book);
        return entity;
    }
}