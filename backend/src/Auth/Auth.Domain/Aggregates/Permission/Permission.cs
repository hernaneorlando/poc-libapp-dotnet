namespace Auth.Domain.Aggregates.Permission;

using Auth.Domain.Enums;

/// <summary>
/// ValueObject representing a Permission in the authorization system.
/// Immutable combination of a Feature and an Action.
/// Example: Book:Create, Book:Update, User:Delete
/// </summary>
public sealed class Permission(PermissionFeature feature, PermissionAction action) : ValueObject
{
    public PermissionFeature Feature { get; } = feature;
    public PermissionAction Action { get; } = action;

    /// <summary>
    /// Gets the permission code (e.g., "Book:Create").
    /// </summary>
    public string Code => $"{Feature}:{Action}";

    /// <summary>
    /// Gets all equality components for ValueObject comparison.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Feature;
        yield return Action;
    }

    public override string ToString() => Code;
}
