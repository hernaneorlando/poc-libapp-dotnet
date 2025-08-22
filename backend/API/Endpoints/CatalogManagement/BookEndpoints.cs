using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Queries;

namespace LibraryApp.API.Endpoints.CatalogManagement;

public static class BookEndpoints
{
    public static WebApplication AddBookEndpoints(this WebApplication app)
    {
        var groupBuilder = CatalogManagementBaseEndpoints.GetGroupBuilder(app, "books");

        CatalogManagementBaseEndpoints
            .AddGetPagedEntitiesEndpoint<GetActiveBooksQuery, BookDto>(groupBuilder, "Books");

        return app;
    }
}