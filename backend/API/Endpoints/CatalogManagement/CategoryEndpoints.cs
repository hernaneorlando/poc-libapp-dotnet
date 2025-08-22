using Application.CatalogManagement.Books.Commands;
using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Queries;

namespace LibraryApp.API.Endpoints.CatalogManagement;

public static class CategoryEndpoints
{
    public static WebApplication AddCategoryEndpoints(this WebApplication app)
    {
        const string EntityName = "Category";
        const string EntityPluralName = "Categories";

        var groupBuilder = CatalogManagementBaseEndpoints.GetGroupBuilder(app, EntityPluralName.ToLower());

        CatalogManagementBaseEndpoints
            .AddCreateEntityEndpoint<CreateCategoryCommand, CategoryDto>(groupBuilder, EntityName);

        CatalogManagementBaseEndpoints
            .AddGetEntityByIdEndpoint<GetCategoryByIdQuery, CategoryDto>(groupBuilder, EntityName);

        CatalogManagementBaseEndpoints
            .AddGetPagedEntitiesEndpoint<GetActiveCategoriesQuery, CategoryDto>(groupBuilder, EntityPluralName);

        CatalogManagementBaseEndpoints
            .AddUpdateEntityEndpoint<UpdateCategoryCommand, CategoryDto>(groupBuilder, EntityName);

        CatalogManagementBaseEndpoints
            .AddDeleteEntityEndpoint<DeleteCategoryCommand, CategoryDto>(groupBuilder, EntityName);

        return app;
    }
}