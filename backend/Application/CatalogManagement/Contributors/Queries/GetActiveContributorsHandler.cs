using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Services;
using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Contributors.Queries;

public class GetActiveContributorsHandler(IContributorService contributorService) : IRequestHandler<GetActiveContributorsQuery, ValidationResult<PagedResponseDTO<ContributorDto>>>
{
    public async Task<ValidationResult<PagedResponseDTO<ContributorDto>>> Handle(GetActiveContributorsQuery request, CancellationToken cancellationToken)
    {
        var contributorResults = await contributorService.GetActiveContributorsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return contributorResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<ContributorDto> { Data = [..contributorResults.Value.Select(contributor => (ContributorDto)contributor)] })
            : ValidationResult.Fail<PagedResponseDTO<ContributorDto>>(contributorResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve contributors.");
    }
}