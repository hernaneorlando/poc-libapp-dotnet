using Application.CatalogManagement.Books.DTOs;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public record CreateCategoryCommand(string Name) : IRequest<ValidationResult<CategoryDto>>
{
    public string? Description { get; set; }
}
