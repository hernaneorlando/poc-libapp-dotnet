using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Services;
using FluentResults;
using MediatR;

namespace Application.CatalogManagement.Publishers.Queries;

public class GetActivePublishersHandler(IPublisherService publisherService) : IRequestHandler<GetActivePublishersQuery, Result<IEnumerable<PublisherDto>>>
{
    public async Task<Result<IEnumerable<PublisherDto>>> Handle(GetActivePublishersQuery request, CancellationToken cancellationToken)
    {
        var publisherResults = await publisherService.GetActivePublishersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return publisherResults.IsSuccess
            ? Result.Ok(publisherResults.Value.Select(publisher => (PublisherDto)publisher))
            : Result.Fail<IEnumerable<PublisherDto>>(publisherResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve publishers.");
    }
}