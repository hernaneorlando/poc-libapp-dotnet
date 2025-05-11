using Domain.CatalogManagement;
using FluentResults;

namespace Application.CatalogManagement.Publishers.Services;

public interface IPublisherService
{
    Task<Result<IEnumerable<Publisher>>> GetActivePublishersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
