using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class CreateCategoryHandler(ICategoryService categoryCommandService) : IRequestHandler<CreateCategoryCommand, ValidationResult<CategoryDto>>
{
    public async Task<ValidationResult<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = Category.CreateCategory(request.Name, request.Description ?? string.Empty);
        if (!result.IsSuccess)
            return ValidationResult.Fail<CategoryDto>(result.Errors);

        var categoryAddedResult = await categoryCommandService.CreateCategoryAsync(result.Value, cancellationToken);
        return categoryAddedResult.IsSuccess
            ? ValidationResult.Ok((CategoryDto)categoryAddedResult.Value)
            : ValidationResult.Fail<CategoryDto>(categoryAddedResult.Errors);
    }
}