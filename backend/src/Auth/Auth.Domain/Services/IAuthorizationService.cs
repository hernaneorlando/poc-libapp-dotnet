namespace Auth.Domain.Services;

using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.User;

/// <summary>
/// Defines the contract for authorization services.
/// Evaluates whether a user has permission to perform an action on a feature.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Determines if a user has permission to perform an action on a feature.
    /// </summary>
    /// <param name="user">The user to check permissions for</param>
    /// <param name="feature">The feature being accessed</param>
    /// <param name="action">The action being performed</param>
    /// <returns>True if the user has permission; false otherwise</returns>
    bool IsGranted(User user, PermissionFeature feature, PermissionAction action);

    /// <summary>
    /// Gets all permissions available to a user (from all roles, minus denied).
    /// </summary>
    /// <param name="user">The user to get permissions for</param>
    /// <returns>Collection of Permission ValueObjects the user can perform</returns>
    IEnumerable<Permission> GetUserPermissions(User user);

    /// <summary>
    /// Gets all permissions available through a user's roles.
    /// </summary>
    /// <param name="user">The user to get role permissions for</param>
    /// <returns>Collection of Permission ValueObjects from the user's roles</returns>
    IEnumerable<Permission> GetRolePermissions(User user);
}
