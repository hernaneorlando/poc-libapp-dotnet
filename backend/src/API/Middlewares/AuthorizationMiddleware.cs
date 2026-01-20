using Auth.Domain.Attributes;
using Auth.Domain.Services;
using Auth.Domain.Repositories;
using System.Security.Claims;

namespace LibraryApp.API.Middlewares;

/// <summary>
/// Middleware for processing RequirePermissionAttribute on endpoints.
/// Automatically enforces permission checks without manual controller code.
/// Implements PBAC (Permission-Based Access Control) pattern.
/// </summary>
public class AuthorizationMiddleware(
    RequestDelegate _next,
    ILogger<AuthorizationMiddleware> _logger)
{
    public async Task InvokeAsync(
        HttpContext context, 
        IAuthorizationService authorizationService,
        IUserRepository userRepository)
    {
        var endpoint = context.GetEndpoint();
        var requirePermission = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (requirePermission != null)
        {
            _logger.LogInformation(
                "Authorization required for {Feature}:{Action}",
                requirePermission.Feature,
                requirePermission.Action);

            // Extract user ID from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Unauthorized: Missing or invalid user ID in claims");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Missing or invalid user ID" });
                return;
            }

            try
            {
                // Load user from repository with roles and permissions
                var userIdValueObject = Auth.Domain.Aggregates.User.UserId.From(userId);
                var user = await userRepository.GetByIdAsync(userIdValueObject, CancellationToken.None);

                if (user is null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: User not found" });
                    return;
                }

                // Check if user has the required permission
                var hasPermission = authorizationService.IsGranted(
                    user,
                    requirePermission.Feature,
                    requirePermission.Action);

                if (!hasPermission)
                {
                    _logger.LogWarning(
                        "Permission denied for user {UserId}: {Feature}:{Action}",
                        userId,
                        requirePermission.Feature,
                        requirePermission.Action);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        error = $"Forbidden: You do not have permission to {requirePermission.Action} {requirePermission.Feature}" 
                    });
                    return;
                }

                _logger.LogInformation(
                    "Permission granted for user {UserId}: {Feature}:{Action}",
                    userId,
                    requirePermission.Feature,
                    requirePermission.Action);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking permissions for user {UserId}",
                    userId);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error during permission check" });
                return;
            }
        }

        await _next(context);
    }
}
