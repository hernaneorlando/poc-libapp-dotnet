using Application.CatalogManagement.Publishers.DTOs;
using Application.SeedWork.MediatR;

namespace Application.CatalogManagement.Publishers.Queries;

public record GetActivePublishersQuery : BasePagedQuery<PublisherDto>;
