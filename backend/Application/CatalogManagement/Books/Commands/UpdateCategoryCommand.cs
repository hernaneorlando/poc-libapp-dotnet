using Application.CatalogManagement.Books.DTOs;
using Application.Common;

namespace Application.CatalogManagement.Books.Commands;

public record UpdateCategoryCommand : UpdateEntityBaseCommand<CategoryDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

