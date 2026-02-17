namespace LibraryApp.Web.Model.Auth;

/// <summary>
/// Helper class for building permission strings from enumerators.
/// Provides type-safe permission construction and commonly used permission constants.
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Builds a permission string in the format "Feature:Action".
    /// </summary>
    /// <param name="feature">The feature/resource being protected</param>
    /// <param name="action">The action being performed</param>
    /// <returns>Permission string (e.g., "Book:Create")</returns>
    public static string Build(PermissionFeature feature, PermissionAction action)
    {
        return $"{feature}:{action}";
    }

    // ==================== Book Permissions ====================
    public static readonly string BookCreate = Build(PermissionFeature.Book, PermissionAction.Create);
    public static readonly string BookRead = Build(PermissionFeature.Book, PermissionAction.Read);
    public static readonly string BookUpdate = Build(PermissionFeature.Book, PermissionAction.Update);
    public static readonly string BookDelete = Build(PermissionFeature.Book, PermissionAction.Delete);

    // ==================== Category Permissions ====================
    public static readonly string CategoryCreate = Build(PermissionFeature.Category, PermissionAction.Create);
    public static readonly string CategoryRead = Build(PermissionFeature.Category, PermissionAction.Read);
    public static readonly string CategoryUpdate = Build(PermissionFeature.Category, PermissionAction.Update);
    public static readonly string CategoryDelete = Build(PermissionFeature.Category, PermissionAction.Delete);

    // ==================== Contributor Permissions ====================
    public static readonly string ContributorCreate = Build(PermissionFeature.Contributor, PermissionAction.Create);
    public static readonly string ContributorRead = Build(PermissionFeature.Contributor, PermissionAction.Read);
    public static readonly string ContributorUpdate = Build(PermissionFeature.Contributor, PermissionAction.Update);
    public static readonly string ContributorDelete = Build(PermissionFeature.Contributor, PermissionAction.Delete);

    // ==================== Publisher Permissions ====================
    public static readonly string PublisherCreate = Build(PermissionFeature.Publisher, PermissionAction.Create);
    public static readonly string PublisherRead = Build(PermissionFeature.Publisher, PermissionAction.Read);
    public static readonly string PublisherUpdate = Build(PermissionFeature.Publisher, PermissionAction.Update);
    public static readonly string PublisherDelete = Build(PermissionFeature.Publisher, PermissionAction.Delete);

    // ==================== BookCheckout Permissions ====================
    public static readonly string BookCheckoutCreate = Build(PermissionFeature.BookCheckout, PermissionAction.Create);
    public static readonly string BookCheckoutRead = Build(PermissionFeature.BookCheckout, PermissionAction.Read);
    public static readonly string BookCheckoutUpdate = Build(PermissionFeature.BookCheckout, PermissionAction.Update);
    public static readonly string BookCheckoutDelete = Build(PermissionFeature.BookCheckout, PermissionAction.Delete);

    // ==================== AuditEntry Permissions ====================
    public static readonly string AuditEntryCreate = Build(PermissionFeature.AuditEntry, PermissionAction.Create);
    public static readonly string AuditEntryRead = Build(PermissionFeature.AuditEntry, PermissionAction.Read);
    public static readonly string AuditEntryUpdate = Build(PermissionFeature.AuditEntry, PermissionAction.Update);
    public static readonly string AuditEntryDelete = Build(PermissionFeature.AuditEntry, PermissionAction.Delete);

    // ==================== User Permissions ====================
    public static readonly string UserCreate = Build(PermissionFeature.User, PermissionAction.Create);
    public static readonly string UserRead = Build(PermissionFeature.User, PermissionAction.Read);
    public static readonly string UserUpdate = Build(PermissionFeature.User, PermissionAction.Update);
    public static readonly string UserDelete = Build(PermissionFeature.User, PermissionAction.Delete);

    // ==================== Role Permissions ====================
    public static readonly string RoleCreate = Build(PermissionFeature.Role, PermissionAction.Create);
    public static readonly string RoleRead = Build(PermissionFeature.Role, PermissionAction.Read);
    public static readonly string RoleUpdate = Build(PermissionFeature.Role, PermissionAction.Update);
    public static readonly string RoleDelete = Build(PermissionFeature.Role, PermissionAction.Delete);

    // ==================== Permission Permissions ====================
    public static readonly string PermissionCreate = Build(PermissionFeature.Permission, PermissionAction.Create);
    public static readonly string PermissionRead = Build(PermissionFeature.Permission, PermissionAction.Read);
    public static readonly string PermissionUpdate = Build(PermissionFeature.Permission, PermissionAction.Update);
    public static readonly string PermissionDelete = Build(PermissionFeature.Permission, PermissionAction.Delete);

    public static bool ContainsRole(this UserLoginInfoDto user, string role)
    {
        return user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static bool ContainsPermission(this UserLoginInfoDto user, string permission)
    {
        return user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}