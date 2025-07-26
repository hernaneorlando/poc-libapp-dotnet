using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesHandler(ICategoryService categoryService) : IRequestHandler<GetActiveCategoriesQuery, ValidationResult<IEnumerable<CategoryDto>>>
{
    public async Task<ValidationResult<IEnumerable<CategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await categoryService.GetActiveCategoriesAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}