using Application.CatalogManagement.Books.DTOs;
using Application.Common.FluentValidation;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesValidator : BasePagedQueryValidator<GetActiveCategoriesQuery, CategoryDto>;