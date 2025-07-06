using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Commands;

public class CreateCategoryHandler(ICategoryService categoryCommandService) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category();
        var notification = category.Create(request.Name, request.Description);
        if (notification.HasErrors)
            return Result.Fail<CategoryDto>(notification.Errors.FirstOrDefault() ?? "Failed to create category.");

        var categoryResult = await categoryCommandService.CreateCategoryAsync(category, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok((CategoryDto)categoryResult.Value)
            : Result.Fail<CategoryDto>(categoryResult.Errors.FirstOrDefault()?.Message ?? "Failed to create category.");
    }
}