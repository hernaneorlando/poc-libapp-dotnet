using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesHandler(ICategoryService categoryService) : IRequestHandler<GetActiveCategoriesQuery, ValidationResult<IEnumerable<CategoryDto>>>
{
    public async Task<ValidationResult<IEnumerable<CategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categoryResults = await categoryService.GetActiveCategoriesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return categoryResults.IsSuccess
            ? ValidationResult.Ok(categoryResults.Value)
            : ValidationResult.Fail<IEnumerable<CategoryDto>>(categoryResults.Errors);
    }
}