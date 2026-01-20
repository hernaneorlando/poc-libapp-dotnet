namespace Auth.Application.Users.Commands.RemoveDeniedPermission;

using FluentValidation;
using Auth.Domain.Enums;

/// <summary>
/// Validator for RemoveDeniedPermissionCommand.
/// </summary>
public sealed class RemoveDeniedPermissionCommandValidator : AbstractValidator<RemoveDeniedPermissionCommand>
{
    public RemoveDeniedPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("User ID must be a valid GUID");

        RuleFor(x => x.Feature)
            .NotEmpty()
            .WithMessage("Feature is required")
            .Must(f => Enum.TryParse<PermissionFeature>(f, ignoreCase: true, out _))
            .WithMessage("Invalid feature. Must be a valid PermissionFeature enum value");

        RuleFor(x => x.Action)
            .NotEmpty()
            .WithMessage("Action is required")
            .Must(a => Enum.TryParse<PermissionAction>(a, ignoreCase: true, out _))
            .WithMessage("Invalid action. Must be a valid PermissionAction enum value");
    }
}
