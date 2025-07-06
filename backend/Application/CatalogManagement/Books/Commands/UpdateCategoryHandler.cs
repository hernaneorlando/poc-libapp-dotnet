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
        var (externalId, notification) = Category.ParseExternalId(request.Id);
        if (notification.HasErrors)
            return Result.Fail<CategoryDto>(notification.Errors);

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(externalId, cancellationToken);
        if (persistedCategoryResult.IsFailed)
            return Result.Fail<CategoryDto>(persistedCategoryResult.Errors);

        var persistedCategory = persistedCategoryResult.Value;
        notification = persistedCategory.Update(request.Name, request.Description);
        if (notification.HasErrors)
            return Result.Fail<CategoryDto>(notification.Errors);

        notification = persistedCategory.ValidateModel();
        if (notification.HasErrors)
            return Result.Fail<CategoryDto>(notification.Errors);

        var categoryResult = await categoryService.UpdateCategoryAsync(persistedCategory, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok((CategoryDto)categoryResult.Value)
            : Result.Fail<CategoryDto>(categoryResult.Errors.FirstOrDefault()?.Message ?? "Failed to update category.");
    }
}