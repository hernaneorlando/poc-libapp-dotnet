using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class UpdateCategoryHandler(ICategoryService categoryService) : IRequestHandler<UpdateCategoryCommand, ValidationResult<CategoryDto>>
{
    public async Task<ValidationResult<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category();
        var result = category.UpdateCategory(request.Id, request.Name, request.Description);
        if (!result.IsSuccess)
            return ValidationResult.Fail<CategoryDto>(result.Errors);

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(category.ExternalId, cancellationToken);
        if (!persistedCategoryResult.IsSuccess)
            return ValidationResult.Fail<CategoryDto>(persistedCategoryResult.Errors);
            
        var persistedCategory = persistedCategoryResult.Value;
        result = persistedCategory.UpdateCategory(category);
        if (!result.IsSuccess)
            return ValidationResult.Fail<CategoryDto>(result.Errors);

        var categoryUpdatedResult = await categoryService.UpdateCategoryAsync(result.Value, cancellationToken);
        return categoryUpdatedResult.IsSuccess
            ? ValidationResult.Ok((CategoryDto)categoryUpdatedResult.Value)
            : ValidationResult.Fail<CategoryDto>(categoryUpdatedResult.Errors);
    }
}