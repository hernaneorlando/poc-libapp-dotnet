using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Services;
using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.CatalogManagement.Publishers.Queries;

public class GetActivePublishersHandler(IPublisherService publisherService) : IRequestHandler<GetActivePublishersQuery, ValidationResult<PagedResponseDTO<PublisherDto>>>
{
    public async Task<ValidationResult<PagedResponseDTO<PublisherDto>>> Handle(GetActivePublishersQuery request, CancellationToken cancellationToken)
    {
        var publisherResults = await publisherService.GetActivePublishersAsync(request.PageNumber, request.PageSize, cancellationToken);
        return publisherResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<PublisherDto> { Data = [..publisherResults.Value.Select(publisher => (PublisherDto)publisher)] })
            : ValidationResult.Fail<PagedResponseDTO<PublisherDto>>(publisherResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve publishers.");
    }
}