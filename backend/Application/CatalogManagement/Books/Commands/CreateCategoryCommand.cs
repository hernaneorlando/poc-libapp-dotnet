using Application.CatalogManagement.Books.DTOs;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public record CreateCategoryCommand(string Name) : IRequest<Result<CategoryDto>>
{
    public string? Description { get; set; }
}
