namespace Auth.Infrastructure;

using Auth.Application.Common.Security;
using Auth.Domain;
using Auth.Domain.Services;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
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
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services;
    }

    /// <summary>
    /// Adds JWT configuration to the dependency injection container.
    /// Must be called after AddAuthInfrastructure.
    /// Reads JWT settings from IConfiguration.
    /// </summary>
    /// <param name="services">The service collection to extend</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind JWT settings from configuration
        var jwtSettings = new JwtSettings
        {
            Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer not configured"),
            Audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT:Audience not configured"),
            SecretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey not configured"),
            TokenExpiryInMinutes = int.Parse(configuration["Jwt:TokenExpiryInMinutes"] ?? "15"),
            RefreshTokenExpiryInDays = int.Parse(configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7")
        };

        services.AddSingleton(jwtSettings);

        return services;
    }
}
