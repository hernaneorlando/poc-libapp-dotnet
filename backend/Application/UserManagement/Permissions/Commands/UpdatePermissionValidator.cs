using Domain.Common.Util;
using FluentValidation;

namespace Application.UserManagement.Permissions.Commands;

public class UpdatePermissionValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id must not be empty.")
            .Must(ValidatorUtil.IsValidGuid).WithMessage("Id must be a valid GUID.");

        RuleFor(command => command.Description)
            .NotEmpty().WithMessage("Permission description is required.")
            .MaximumLength(100).WithMessage("Permission description must not exceed 100 characters.");
    }
}