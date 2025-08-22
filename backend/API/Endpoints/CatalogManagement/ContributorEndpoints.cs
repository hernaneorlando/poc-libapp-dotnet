using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Queries;

namespace LibraryApp.API.Endpoints.CatalogManagement;

public static class ContributorEndpoints
{
    public static WebApplication AddContributorsEndpoints(this WebApplication app)
    {
        var groupBuilder = CatalogManagementBaseEndpoints.GetGroupBuilder(app, "contributors");

        CatalogManagementBaseEndpoints
            .AddGetPagedEntitiesEndpoint<GetActiveContributorsQuery, ContributorDto>(groupBuilder, "Contributors");

        return app;
    }
}
