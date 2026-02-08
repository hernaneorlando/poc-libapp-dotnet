namespace Auth.Domain.Services;

using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.User;

/// <summary>
/// Default implementation of authorization service.
/// Evaluates permissions based on: Denied (highest) > Role Permissions (default).
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    /// <summary>
    /// Determines if a user has permission to perform an action on a feature.
    /// Priority: Explicit Deny (highest) > Role Permissions (default)
    /// </summary>
    public bool IsGranted(User user, PermissionFeature feature, PermissionAction action)
    {
        if (user is null)
            return false;

        var permission = new Permission(feature, action);
        
        // 1. Explicit Deny wins (highest priority)
        if (user.DeniedPermissions.Contains(permission))
            return false;

        // 2. Role-based permissions (default)
        return user.Roles.Any(role => role.Permissions.Contains(permission));
    }

    /// <summary>
    /// Gets all permissions available to a user (from all roles, minus denied).
    /// </summary>
    public IEnumerable<Permission> GetUserPermissions(User user)
    {
        if (user is null)
            return [];

        var allRolePermissions = user.Roles
            .SelectMany(role => role.Permissions)
            .Distinct();

        return allRolePermissions
            .Except(user.DeniedPermissions)
            .Distinct();
    }

    /// <summary>
    /// Gets all permissions available through a user's roles.
    /// </summary>
    public IEnumerable<Permission> GetRolePermissions(User user)
    {
        if (user is null)
            return [];

        return user.Roles
            .SelectMany(role => role.Permissions)
            .Distinct();
    }
}