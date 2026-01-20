namespace Auth.Domain.Attributes;

/// <summary>
/// Declarative attribute to specify required permissions for an endpoint.
/// Processed by AuthorizationMiddleware.
/// </summary>
/// <remarks>
/// Creates a new permission requirement.
/// </remarks>
/// <param name="feature">The feature being protected</param>
/// <param name="action">The action being protected</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute(PermissionFeature feature, PermissionAction action) : Attribute
{
    /// <summary>
    /// The feature that requires permission.
    /// </summary>
    public PermissionFeature Feature { get; } = feature;

    /// <summary>
    /// The action that requires permission.
    /// </summary>
    public PermissionAction Action { get; } = action;
}
