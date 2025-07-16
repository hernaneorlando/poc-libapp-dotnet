using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class DeleteCategoryHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category();
        var result = category.Deactivate(request.Id);
        if (!result.IsSuccess)
            return Result.Fail(result.Errors);

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(category.ExternalId, cancellationToken);
        if (!persistedCategoryResult.IsSuccess)
            return Result.Fail(persistedCategoryResult.Errors);

        var persistedCategory = persistedCategoryResult.Value;
        result = persistedCategory.Deactivate(category);
        if (!result.IsSuccess)
            return Result.Fail(result.Errors);

        var categoryResult = await categoryService.UpdateCategoryAsync(persistedCategory, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok()
            : Result.Fail(categoryResult.Errors);
    }
}