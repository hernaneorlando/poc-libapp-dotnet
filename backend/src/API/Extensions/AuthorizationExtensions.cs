using Auth.Domain.Services;

namespace LibraryApp.API.Extensions;

/// <summary>
/// Extension methods for registering authorization services.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers authorization services in the dependency injection container.
    /// </summary>
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        return services;
    }
}
