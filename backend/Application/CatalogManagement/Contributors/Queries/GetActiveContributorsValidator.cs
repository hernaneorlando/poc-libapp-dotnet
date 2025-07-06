using Application.CatalogManagement.Contributors.DTOs;
using Application.SeedWork.FluentValidation;

namespace Application.CatalogManagement.Contributors.Queries;

public class GetActiveContributorsValidator : BasePagedQueryValidator<GetActiveContributorsQuery, ContributorDto>;