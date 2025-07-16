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

        var categoryResult = await categoryCommandService.CreateCategoryAsync(result.Value, cancellationToken);
        return categoryResult.IsSuccess
            ? ValidationResult.Ok((CategoryDto)categoryResult.Value)
            : ValidationResult.Fail<CategoryDto>(categoryResult.Errors);
    }
}