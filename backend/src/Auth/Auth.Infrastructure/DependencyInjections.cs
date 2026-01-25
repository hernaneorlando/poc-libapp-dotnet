namespace Auth.Infrastructure;

using Auth.Domain;
using Auth.Domain.Services;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Dependency injection extensions for the Auth infrastructure layer.
/// Registers EF Core DbContext, repository implementations, and security services.
/// </summary>
public static class DependencyInjections
{
    /// <summary>
    /// Adds Auth infrastructure services to the dependency injection container.
    /// Configures SQL Server with Entity Framework Core, repositories, and security services.
    /// </summary>
    /// <param name="services">The service collection to extend</param>
    /// <param name="connectionString">SQL Server connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Register DbContext
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.DefaultSchema);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
                });

            // Enable query tracking prevention for read-only queries
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register security services
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services;
    }
}
