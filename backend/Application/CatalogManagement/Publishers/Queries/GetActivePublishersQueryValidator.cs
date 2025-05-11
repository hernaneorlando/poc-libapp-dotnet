using Application.CatalogManagement.Publishers.DTOs;
using Application.SeedWork.FluentValidation;

namespace Application.CatalogManagement.Publishers.Queries;

public class GetActivePublishersQueryValidator : BasePagedQueryValidator<GetActivePublishersQuery, PublisherDto>;