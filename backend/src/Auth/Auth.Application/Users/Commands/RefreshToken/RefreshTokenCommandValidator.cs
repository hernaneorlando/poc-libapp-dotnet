namespace Auth.Application.Users.Commands.RefreshToken;

using FluentValidation;

/// <summary>
/// Validator for RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .Length(80, 200)
            .WithMessage("Refresh token must be between 80 and 200 characters (should be base64 encoded)");
    }
}
