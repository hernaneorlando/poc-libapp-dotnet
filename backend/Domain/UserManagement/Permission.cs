using Domain.Common;
using Domain.UserManagement.Enums;

namespace Domain.UserManagement;

public sealed class Permission : DocumentDbModel<Permission>
{
    public PermissionFeature Feature { get; set; }
    public PermissionAction Action { get; set; }
    public string Description { get; set; } = string.Empty;

    public string Code => $"{Feature}:{Action}";

    private Permission(PermissionFeature feature, PermissionAction action)
    {
        Feature = feature;
        Action = action;
    }

    public static Permission Create(PermissionFeature feature, PermissionAction action, string description) =>
        new(feature, action) { Description = description };

    public static ValidationResult<Permission> Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ValidationResult.Fail<Permission>("Permission cannot be created without code.");

        var result = ValidationResult.Create<Permission>();

        var splittedCode = code.Split(":");
        if (splittedCode.Length != 2)
            return ValidationResult.Fail<Permission>("Invalid Permission code format");

        if (!Enum.TryParse<PermissionFeature>(splittedCode[0], out var feature))
            result.AddError("Error creating Permission due to non-existent feature.");

        if (!Enum.TryParse<PermissionAction>(splittedCode[1], out var action))
            result.AddError("Error creating Permission due to non-existent action.");

        if (result.IsFailure)
            return result;

        var permission = new Permission(feature, action)
        {
            Description = description
        };

        permission.Validate(result);
        result.AddValue(permission);
        return result;
    }

    public ValidationResult<Permission> Update(Guid externalId, string description)
    {
        var result = ValidationResult.Create<Permission>();

        if (ExternalId != externalId)
        {
            result.AddError("Permissions must have the same External Id");
            return result;
        }

        Description = description;
        UpdatedAt = DateTime.UtcNow;

        Validate(result);
        result.AddValue(this);
        return result;
    }

    private ValidationResult<Permission> Validate(ValidationResult<Permission>? result = null)
    {
        result ??= ValidationResult.Create<Permission>();

        if (Description is not null && Description.Length > 256)
            result.AddError("Permission Description must not exceed 256 characters");

        return result;
    }
}