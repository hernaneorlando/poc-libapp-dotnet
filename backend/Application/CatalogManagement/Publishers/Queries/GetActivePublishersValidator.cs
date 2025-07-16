using Application.CatalogManagement.Publishers.DTOs;
using Application.Common.FluentValidation;

namespace Application.CatalogManagement.Publishers.Queries;

public class GetActivePublishersValidator : BasePagedQueryValidator<GetActivePublishersQuery, PublisherDto>;