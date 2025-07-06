using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Books.Queries;

public class GetCategoryByIdHandler(ICategoryService categoryService) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var (ExternalId, Notification) = Category.ParseExternalId(request.Id);
        if (Notification.HasErrors)
            return Result.Fail<CategoryDto>(Notification.Errors);

        var categoryResult = await categoryService.GetCategoryDtoByIdAsync(ExternalId, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok(categoryResult.Value)
            : Result.Fail<CategoryDto>(categoryResult.Errors.FirstOrDefault()?.Message ?? $"Category with ID {request.Id} not found.");
    }
}