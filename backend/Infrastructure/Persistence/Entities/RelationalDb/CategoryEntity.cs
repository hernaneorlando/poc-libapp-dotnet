using Application.CatalogManagement.Books.DTOs;
using Domain.CatalogManagement;
using Domain.Common;
using Infrastructure.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class CategoryEntity : RelationalDbBaseBaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IList<BookEntity> Books { get; set; } = [];

    public static implicit operator CategoryDto(CategoryEntity entity)
    {
        var dto = new CategoryDto(entity.Name)
        {
            Description = entity.Description
        };

        dto.ConvertEntityBaseProperties(entity);
        return dto;
    }

    public static implicit operator Category(CategoryEntity entity)
    {
        var modelResult = Category.Create(entity.Name, entity.Description ?? string.Empty);
        if (!modelResult.IsSuccess)
            throw new ValidationException(modelResult.Errors);

        var model = modelResult.Value;
        model.Books = [.. entity.Books.Select(b => (Book)b)];
        model.ConvertEntityBaseProperties(entity);
        return model;
    }
    
    public static implicit operator CategoryEntity(Category category)
    {
        var entity = new CategoryEntity()
        {
            Name = category.Name,
            Description = category.Description,
            Books = [.. category.Books.Select(b => (BookEntity)b)],
        };

        entity.ConvertModelBaseProperties(category);
        return entity;
    }
}