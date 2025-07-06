using Application.CatalogManagement.Books.DTOs;
using Application.SeedWork.FluentValidation;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesValidator : BasePagedQueryValidator<GetActiveCategoriesQuery, CategoryDto>;