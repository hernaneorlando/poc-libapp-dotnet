using Application.Common.BaseDTO;
using Domain.CatalogManagement;

namespace Application.CatalogManagement.Books.DTOs;

public record CategoryDto(string Name) : BaseDto
{
    public string? Description { get; set; }
    
    public static implicit operator CategoryDto(Category category)
    {
        var categoryDto = new CategoryDto(category.Name)
        {
            Description = category.Description
        };

        categoryDto.ConvertBaseProperties(category);
        return categoryDto;
    }
}