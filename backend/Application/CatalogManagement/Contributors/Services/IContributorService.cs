using Domain.CatalogManagement;
using FluentResults;

namespace Application.CatalogManagement.Contributors.Services;

public interface IContributorService
{
    Task<Result<IEnumerable<Contributor>>> GetActiveContributorsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}