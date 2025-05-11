using Application.CatalogManagement.Contributors.DTOs;
using Application.SeedWork.MediatR;

namespace Application.CatalogManagement.Contributors.Queries;

public record GetActiveContributorsQuery : BasePagedQuery<ContributorDto>;