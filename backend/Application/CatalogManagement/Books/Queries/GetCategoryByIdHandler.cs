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
        var externalIdResult = Category.ParseExternalId(request.Id);
        if (externalIdResult.IsSuccess)
            return Result.Fail<CategoryDto>(externalIdResult.Errors);

        var categoryResult = await categoryService.GetCategoryDtoByIdAsync(externalIdResult.Value, cancellationToken);
        return categoryResult.IsSuccess
            ? Result.Ok(categoryResult.Value)
            : Result.Fail<CategoryDto>(categoryResult.Errors);
    }
}