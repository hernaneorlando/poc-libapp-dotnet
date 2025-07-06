using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class DeleteCategoryHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var (externalId, notification) = Category.ParseExternalId(request.Id);
        if (notification.HasErrors)
            return Result.Fail(notification.Errors);

        var result = await categoryService.DeleteCategoryAsync(externalId, cancellationToken);
        return result.IsSuccess
           ? Result.Ok()
           : Result.Fail(result.Errors.FirstOrDefault()?.Message ?? "Failed to delete category.");
    }
}