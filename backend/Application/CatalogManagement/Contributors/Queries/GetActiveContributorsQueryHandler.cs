using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Services;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Contributors.Queries;

public class GetActiveContributorsQueryHandler(IContributorService contributorService) : IRequestHandler<GetActiveContributorsQuery, Result<IEnumerable<ContributorDto>>>
{
    private readonly IContributorService contributorService = contributorService;

    public async Task<Result<IEnumerable<ContributorDto>>> Handle(GetActiveContributorsQuery request, CancellationToken cancellationToken)
    {
        var contributorResults = await contributorService.GetActiveContributorsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return contributorResults.IsSuccess
            ? Result.Ok(contributorResults.Value.Select(contributor => (ContributorDto)contributor))
            : Result.Fail<IEnumerable<ContributorDto>>(contributorResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve contributors.");
    }
}