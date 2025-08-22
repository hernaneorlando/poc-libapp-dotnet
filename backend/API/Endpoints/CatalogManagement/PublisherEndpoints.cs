using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Queries;

namespace LibraryApp.API.Endpoints.CatalogManagement;

public static class PublisherEndpoints
{
    public static WebApplication AddPublisherEndpoints(this WebApplication app)
    {
        var groupBuilder = CatalogManagementBaseEndpoints.GetGroupBuilder(app, "publishers");

        CatalogManagementBaseEndpoints
            .AddGetPagedEntitiesEndpoint<GetActivePublishersQuery, PublisherDto>(groupBuilder, "Publishers");

        return app;
    }
}