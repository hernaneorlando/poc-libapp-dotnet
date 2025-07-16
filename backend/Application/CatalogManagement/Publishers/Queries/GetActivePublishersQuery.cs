using Application.CatalogManagement.Publishers.DTOs;
using Application.Common.MediatR;

namespace Application.CatalogManagement.Publishers.Queries;

public record GetActivePublishersQuery : BasePagedQuery<PublisherDto>;
