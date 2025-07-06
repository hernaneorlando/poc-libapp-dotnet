using Application.CatalogManagement.Books.DTOs;
using Application.SeedWork.MediatR;

namespace Application.CatalogManagement.Books.Queries;

public record GetCategoryByIdQuery : BaseGetByIdQuery<CategoryDto>;