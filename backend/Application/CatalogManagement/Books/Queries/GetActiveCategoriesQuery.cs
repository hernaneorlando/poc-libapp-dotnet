using Application.CatalogManagement.Books.DTOs;
using Application.Common.MediatR;

namespace Application.CatalogManagement.Books.Queries;

public record GetActiveCategoriesQuery : BasePagedQuery<CategoryDto>;