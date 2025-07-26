using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class DeleteCategoryHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, ValidationResult>
{
    public async Task<ValidationResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create();
        var result = category.Deactivate(request.Id);
        if (!result.IsSuccess)
            return result;

        var persistedCategoryResult = await categoryService.GetCategoryByIdAsync(category.ExternalId, cancellationToken);
        if (!persistedCategoryResult.IsSuccess)
            return persistedCategoryResult;

        var persistedCategory = persistedCategoryResult.Value;
        result = persistedCategory.Deactivate(category);
        if (!result.IsSuccess)
            return result;

        return await categoryService.UpdateCategoryAsync(persistedCategory, cancellationToken);
    }
}