namespace LibraryApp.Web.Model.AuthManagement.Enums;

/// <summary>
/// Represents the features/resources that can be protected by permissions.
/// </summary>
public enum PermissionFeature
{
    /// <summary>Book catalog management</summary>
    Book = 1,

    /// <summary>Book category management</summary>
    Category = 2,

    /// <summary>Book contributor management</summary>
    Contributor = 3,

    /// <summary>Publisher management</summary>
    Publisher = 4,

    /// <summary>Book checkout/loan management</summary>
    BookCheckout = 5,

    /// <summary>Audit log and entry management</summary>
    AuditEntry = 6,

    /// <summary>User account management</summary>
    User = 7,

    /// <summary>Role management</summary>
    Role = 8,

    /// <summary>Permission management</summary>
    Permission = 9
}
