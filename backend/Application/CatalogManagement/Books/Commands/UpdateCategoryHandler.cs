using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class UpdateCategoryHandler(ICategoryService categoryService) : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category();
        var result = category.UpdateCategory(request.Id, request.Name, request.Description);
        if (!result.IsSuccess)
            return Result.Fail<CategoryDto>(result.Errors);

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(category.ExternalId, cancellationToken);
        if (!persistedCategoryResult.IsSuccess)
            return Result.Fail<CategoryDto>(persistedCategoryResult.Errors);
            
        var persistedCategory = persistedCategoryResult.Value;
        result = persistedCategory.UpdateCategory(category);
        if (!result.IsSuccess)
            return Result.Fail<CategoryDto>(result.Errors);

        var categoryResult = await categoryService.UpdateCategoryAsync(persistedCategory, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok((CategoryDto)categoryResult.Value)
            : Result.Fail<CategoryDto>(categoryResult.Errors);
    }
}