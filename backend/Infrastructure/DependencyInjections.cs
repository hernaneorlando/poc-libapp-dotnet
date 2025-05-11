using Application.CatalogManagement.Books.Services;
using Application.CatalogManagement.Contributors.Services;
using Application.CatalogManagement.Publishers.Services;
using Application.LoanManagement.BookCheckouts.Services;
using Application.ReportManagement.AuditEntries.Services;
using Application.UserManagement.Permissions.Services;
using Application.UserManagement.Roles.Services;
using Application.UserManagement.Users.Services;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureDependencyInjections
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddTransient<IAuditEntryService, AuditEntryService>();
        services.AddTransient<IBookCheckoutService, BookCheckoutService>();
        services.AddTransient<IBookService, BookService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IContributorService, ContributorService>();
        services.AddTransient<IPermissionService, PermissionService>();
        services.AddTransient<IPublisherService, PublisherService>();
        services.AddTransient<IRoleService, RoleService>();
        services.AddTransient<IUserService, UserService>();

        return services;
    }
}
