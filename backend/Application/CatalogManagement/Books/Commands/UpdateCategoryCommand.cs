using System.Text.Json.Serialization;
using Application.CatalogManagement.Books.DTOs;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public record UpdateCategoryCommand : IRequest<ValidationResult<CategoryDto>>
{
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
