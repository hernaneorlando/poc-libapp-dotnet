using Application.CatalogManagement.Publishers.Services;
using Domain.CatalogManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PublisherService(SqlDataContext sqlDataContext) : IPublisherService
{
    private readonly SqlDataContext sqlDataContext = sqlDataContext;

    public async Task<Result<IEnumerable<Publisher>>> GetActivePublishersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var publishers = await sqlDataContext.Publishers
            .Where(p => p.Active)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (publishers is null || publishers.Count == 0)
        {
            return Result.Fail("No active publishers found.");
        }

        return Result.Ok(publishers.Select(publisher => (Publisher)publisher));
    }
}