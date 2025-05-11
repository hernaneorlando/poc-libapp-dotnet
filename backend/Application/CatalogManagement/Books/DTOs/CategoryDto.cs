using Application.Common;
using Application.SeedWork.BaseDTO;
using Domain.CatalogManagement;

namespace Application.CatalogManagement.Books.DTOs;

public record CategoryDto(string Name) : BaseDto
{
    public string? Description { get; set; }
    public IList<BookDto> Books { get; set; } = [];

    public static implicit operator CategoryDto(Category category)
    {
        var categoryDto = new CategoryDto(category.Name)
        {
            Description = category.Description,
            Books = [.. category.Books.Select(b => (BookDto)b)]
        };

        categoryDto.ConvertModelBaseProperties(category);
        return categoryDto;
    }

    public static implicit operator Category(CategoryDto categoryDto)
    {
        var category = new Category()
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            Books = [.. categoryDto.Books.Select(b => (Book)b)]
        };

        category.ConvertDtoBaseProperties(categoryDto);
        return category;
    }
}