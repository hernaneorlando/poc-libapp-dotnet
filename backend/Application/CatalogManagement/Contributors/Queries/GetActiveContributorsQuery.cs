using Application.CatalogManagement.Contributors.DTOs;
using Application.Common.MediatR;

namespace Application.CatalogManagement.Contributors.Queries;

public record GetActiveContributorsQuery : BasePagedQuery<ContributorDto>;