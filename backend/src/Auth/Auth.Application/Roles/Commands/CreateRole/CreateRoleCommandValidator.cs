namespace Auth.Application.Roles.Commands.CreateRole;

using FluentValidation;
using Auth.Domain.Enums;

/// <summary>
/// Validator for CreateRoleCommand.
/// Ensures role name, description, and permissions meet requirements.
/// </summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MinimumLength(3)
            .WithMessage("Role name must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Role name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Role description is required")
            .MinimumLength(10)
            .WithMessage("Role description must be at least 10 characters")
            .MaximumLength(256)
            .WithMessage("Role description must not exceed 256 characters");

        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("Permissions collection cannot be null")
            .Must(permissions => permissions.Count > 0)
            .WithMessage("At least one permission is required");

        RuleForEach(x => x.Permissions)
            .Must(IsValidPermission)
            .WithMessage("Invalid permission feature/action combination");
    }

    /// <summary>
    /// Validates that a permission has valid enum values.
    /// </summary>
    private static bool IsValidPermission(RolePermissionRequest permission)
    {
        // Check if Feature enum value is valid
        if (!Enum.TryParse<PermissionFeature>(permission.Feature, ignoreCase: true, out _))
            return false;

        // Check if Action enum value is valid
        if (!Enum.TryParse<PermissionAction>(permission.Action, ignoreCase: true, out _))
            return false;

        return true;
    }
}
