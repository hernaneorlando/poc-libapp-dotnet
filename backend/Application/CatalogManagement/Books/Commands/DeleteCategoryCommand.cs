using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public record DeleteCategoryCommand(string Id) : IRequest<ValidationResult>;