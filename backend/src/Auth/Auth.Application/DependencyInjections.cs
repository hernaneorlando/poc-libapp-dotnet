namespace Auth.Application;

using Microsoft.Extensions.DependencyInjection;
using Auth.Domain.Services;
using Auth.Application.Services;
using Auth.Application.Common.Security.Interfaces;
using Auth.Application.Common.Security;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Dependency injection configuration for Auth.Application.
/// </summary>
public static class DependencyInjections
{
    /// <summary>
    /// Registers Auth.Application services in the dependency injection container.
    /// </summary>
    public static IServiceCollection AddAuthApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjections).Assembly));
        services.AddScoped<IUsernameGeneratorService, UsernameGeneratorService>();

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
        // Register security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

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
