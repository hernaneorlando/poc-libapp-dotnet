using Application.CatalogManagement.Books.Services;
using Application.CatalogManagement.Contributors.Services;
using Application.CatalogManagement.Publishers.Services;
using Application.LoanManagement.BookCheckouts.Services;
using Application.ReportManagement.AuditEntries.Services;
using Application.UserManagement.Permissions.Services;
using Application.UserManagement.Roles.Services;
using Application.UserManagement.Users.Services;
using Infrastructure.Services.CatalogManagement;
using Infrastructure.Services.LoanManagement;
using Infrastructure.Services.ReportManagement;
using Infrastructure.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureDependencyInjections
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        AddCatalogManagementServices(services);
        AddLoanManagementServices(services);
        AddReportManagement(services);
        AddUserManagementServices(services);
        return services;
    }

    private static void AddCatalogManagementServices(IServiceCollection services)
    {
        services.AddTransient<IBookService, BookService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IContributorService, ContributorService>();
        services.AddTransient<IPublisherService, PublisherService>();
    }

    private static void AddLoanManagementServices(IServiceCollection services)
    {
        services.AddTransient<IBookCheckoutService, BookCheckoutService>();
    }

    private static void AddReportManagement(IServiceCollection services)
    {
        services.AddTransient<IAuditEntryService, AuditEntryService>();
    }

    private static void AddUserManagementServices(IServiceCollection services)
    {
        services.AddTransient<IPermissionService, PermissionService>();
        services.AddTransient<IRoleService, RoleService>();
        services.AddTransient<IUserService, UserService>();
    }
}
