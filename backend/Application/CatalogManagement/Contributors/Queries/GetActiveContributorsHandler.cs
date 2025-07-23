using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Services;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Contributors.Queries;

public class GetActiveContributorsHandler(IContributorService contributorService) : IRequestHandler<GetActiveContributorsQuery, ValidationResult<IEnumerable<ContributorDto>>>
{
    public async Task<ValidationResult<IEnumerable<ContributorDto>>> Handle(GetActiveContributorsQuery request, CancellationToken cancellationToken)
    {
        var contributorResults = await contributorService.GetActiveContributorsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return contributorResults.IsSuccess
            ? ValidationResult.Ok(contributorResults.Value.Select(contributor => (ContributorDto)contributor))
            : ValidationResult.Fail<IEnumerable<ContributorDto>>(contributorResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve contributors.");
    }
}