using Domain.Common.Util;
using FluentValidation;

namespace Application.UserManagement.Permissions.Commands;

public class DeletePermissionValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id must not be empty.")
            .Must(ValidatorUtil.IsValidGuid).WithMessage("Id must be a valid GUID.");
    }
}