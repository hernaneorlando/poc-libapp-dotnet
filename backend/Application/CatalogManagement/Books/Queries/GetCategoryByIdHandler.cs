using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetCategoryByIdHandler(ICategoryService categoryService) : IRequestHandler<GetCategoryByIdQuery, ValidationResult<CategoryDto>>
{
    public async Task<ValidationResult<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var externalIdResult = Category.ParseExternalId(request.Id);
        if (!externalIdResult.IsSuccess)
            return ValidationResult.Fail<CategoryDto>(externalIdResult.Errors);

        var categoryResult = await categoryService.GetCategoryDtoByIdAsync(externalIdResult.Value, cancellationToken);
        return categoryResult.IsSuccess
            ? ValidationResult.Ok(categoryResult.Value)
            : ValidationResult.Fail<CategoryDto>(categoryResult.Errors);
    }
}