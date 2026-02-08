namespace Auth.Application.Users.Commands.Login;

using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Auth.Domain;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Repositories.Interfaces;
using Auth.Infrastructure.Specifications;
using Core.API;
using Core.Validation;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for LoginCommand.
/// Authenticates user, generates JWT token and refresh token.
/// </summary>
public sealed class LoginCommandHandler(
    IUserRepository _userRepository,
    IPasswordHasher _passwordHasher,
    ITokenService _tokenService,
    JwtSettings _jwtSettings,
    ILogger<LoginCommandHandler> _logger,
    IUnitOfWork _unitOfWork) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Step 1: Find user by username
        var user = await _userRepository.FindAsync(new GetUserByUsername(request.Username), cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            throw new ValidationException("Invalid username");
        }

        // Step 2: Verify password
        if (user.PasswordHash is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user - {Username}", request.Username);
            throw new ValidationException("Invalid password");
        }

        // Step 3: Generate JWT access token
        var accessToken = _tokenService.GenerateAccessToken(user);

        // Step 4: Generate refresh token
        var refreshTokenString = _tokenService.GenerateRefreshToken(user.Id.Value);
        var refreshToken = RefreshToken.Create(
            refreshTokenString,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays));

        // Step 5: Add refresh token to user
        user.AddRefreshToken(refreshToken);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Step 6: Persist changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        // Step 7: Build response
        var response = new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresInSeconds: _jwtSettings.TokenExpiryInMinutes * 60,
            User: new UserLoginInfo(
                Id: user.Id.Value,
                Username: user.Username.Value,
                Email: user.Contact.Email,
                FullName: user.GetFullName(),
                Roles: [.. user.Roles.Select(r => r.Name)]));

        return Result<LoginResponse>.Ok(response);
    }
}
