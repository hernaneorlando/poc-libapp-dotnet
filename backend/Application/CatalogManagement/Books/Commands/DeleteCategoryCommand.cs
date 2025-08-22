using Application.Common;

namespace Application.CatalogManagement.Books.Commands;

public record DeleteCategoryCommand(string Id) : DeleteEntityBaseCommand(Id);