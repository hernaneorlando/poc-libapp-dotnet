using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Services;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Publishers.Queries;

public class GetActivePublishersHandler(IPublisherService publisherService) : IRequestHandler<GetActivePublishersQuery, ValidationResult<IEnumerable<PublisherDto>>>
{
    public async Task<ValidationResult<IEnumerable<PublisherDto>>> Handle(GetActivePublishersQuery request, CancellationToken cancellationToken)
    {
        var publisherResults = await publisherService.GetActivePublishersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return publisherResults.IsSuccess
            ? ValidationResult.Ok(publisherResults.Value.Select(publisher => (PublisherDto)publisher))
            : ValidationResult.Fail<IEnumerable<PublisherDto>>(publisherResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve publishers.");
    }
}