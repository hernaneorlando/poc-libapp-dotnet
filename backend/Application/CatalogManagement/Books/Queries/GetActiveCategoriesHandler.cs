using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetActiveCategoriesHandler(ICategoryService categoryService) : IRequestHandler<GetActiveCategoriesQuery, ValidationResult<PagedResponseDTO<CategoryDto>>>
{
    public async Task<ValidationResult<PagedResponseDTO<CategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await categoryService.GetActiveEntitiesAsync(request.PageNumber, request.PageSize, request.OrderBy, request.IsDescending, cancellationToken);
    }
}