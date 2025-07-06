using Application.CatalogManagement.Contributors.Services;
using Domain.CatalogManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.CatalogManagement;

public class ContributorService(SqlDataContext sqlDataContext) : IContributorService
{
    public async Task<Result<IEnumerable<Contributor>>> GetActiveContributorsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var contributors = await sqlDataContext.Contributors
            .Where(a => a.Active)
            .OrderBy(a => a.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (contributors is null || contributors.Count == 0)
        {
            return Result.Fail("No active contributors found.");
        }

        return Result.Ok(contributors.Select(contributor => (Contributor)contributor));
    }
}