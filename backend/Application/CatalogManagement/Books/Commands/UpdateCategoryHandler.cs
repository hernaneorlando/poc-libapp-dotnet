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
        var categoryIdResult = Category.ParseExternalId(request.Id);
        if (categoryIdResult.IsFailure)
            return ValidationResult.Fail<CategoryDto>(categoryIdResult.Errors);

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(categoryIdResult.Value, cancellationToken);
        if (persistedCategoryResult.IsFailure)
            return ValidationResult.Fail<CategoryDto>(persistedCategoryResult.Errors);
            
        var persistedCategory = persistedCategoryResult.Value;
        var result = persistedCategory.Update(categoryIdResult.Value, request.Name, request.Description);
        if (result.IsFailure)
            return ValidationResult.Fail<CategoryDto>(result.Errors);

        var categoryUpdatedResult = await categoryService.UpdateCategoryAsync(result.Value, cancellationToken);
        return categoryUpdatedResult.IsSuccess
            ? ValidationResult.Ok((CategoryDto)categoryUpdatedResult.Value)
            : ValidationResult.Fail<CategoryDto>(categoryUpdatedResult.Errors);
    }
}