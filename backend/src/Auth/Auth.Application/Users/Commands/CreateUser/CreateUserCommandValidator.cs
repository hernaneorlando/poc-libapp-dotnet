namespace Auth.Application.Users.Commands.CreateUser;

using FluentValidation;
using Auth.Domain.Repositories;
using Auth.Domain.Enums;

/// <summary>
/// Validator for CreateUserCommand.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email must be unique");

        RuleFor(x => x.UserType)
            .NotEmpty().WithMessage("User type is required")
            .Must(userType => Enum.TryParse<UserType>(userType, ignoreCase: true, out _))
            .WithMessage("User type must be valid (Customer or Employee)");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.RoleIds)
            .Must(roleIds => roleIds == null || roleIds.Count == 0 || roleIds.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Role IDs must be valid (non-empty strings)");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return existingUser is null;
    }
}
