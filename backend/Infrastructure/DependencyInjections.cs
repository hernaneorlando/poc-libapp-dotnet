using Application.CatalogManagement.Books.Services;
using Application.CatalogManagement.Contributors.Services;
using Application.CatalogManagement.Publishers.Services;
using Application.LoanManagement.BookCheckouts.Services;
using Application.ReportManagement.AuditEntries.Services;
using Application.UserManagement.Permissions.Services;
using Application.UserManagement.Roles.Services;
using Application.UserManagement.Users.Services;
using Infrastructure.Persistence.Context;
using Infrastructure.Services.CatalogManagement;
using Infrastructure.Services.LoanManagement;
using Infrastructure.Services.ReportManagement;
using Infrastructure.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure;

public static class InfrastructureDependencyInjections
{
    public static IServiceCollection AddNoSqlDbContext(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.Configure<MongoDbConfiguration>(configuration);
        services.AddTransient<NoSqlDataContext>();

        return services;
    }

    public static IServiceCollection AddSqlDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SqlDataContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });

        return services;
    }

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
