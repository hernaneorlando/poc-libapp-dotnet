using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetActiveCategoriesQuery, Result<IEnumerable<CategoryDto>>>
{
    private readonly ICategoryService categoryService = categoryService;

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categoryResults = await categoryService.GetActiveCategoriesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return categoryResults.IsSuccess
            ? Result.Ok(categoryResults.Value.Select(category => (CategoryDto)category))
            : Result.Fail<IEnumerable<CategoryDto>>(categoryResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve categories.");
    }
}