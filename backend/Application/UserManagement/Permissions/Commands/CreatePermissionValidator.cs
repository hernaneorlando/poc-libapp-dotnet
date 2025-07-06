using FluentValidation;

namespace Application.UserManagement.Permissions.Commands;

public class CreatePermissionValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Permission code is required.")
            .MaximumLength(20).WithMessage("Permission code must not exceed 20 characters.");

        RuleFor(command => command.Description)
            .MaximumLength(100).WithMessage("Permission description must not exceed 100 characters.");
    }
}
