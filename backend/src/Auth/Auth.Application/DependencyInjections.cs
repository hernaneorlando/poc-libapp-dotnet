namespace Auth.Application;

using Microsoft.Extensions.DependencyInjection;
using Auth.Domain.Services;
using Auth.Application.Services;

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
}
