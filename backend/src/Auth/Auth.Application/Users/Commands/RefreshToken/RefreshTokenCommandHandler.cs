namespace Auth.Application.Users.Commands.RefreshToken;

using MediatR;
using Auth.Domain;
using Auth.Domain.Repositories;
using Auth.Application.Common;
using Auth.Application.Common.Security;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for RefreshTokenCommand.
/// Validates the refresh token and generates a new access token and refresh token.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IUserRepository _userRepository,
    ITokenService _tokenService,
    JwtSettings _jwtSettings,
    IUnitOfWork _unitOfWork,
    ILogger<RefreshTokenCommandHandler> _logger) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to refresh token");

        // Validate command
        var validator = new RefreshTokenCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result<RefreshTokenResponse>.Invalid(validationResult.Errors.Select(e => e.ErrorMessage));

        try
        {
            // Find user by refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Refresh token not found or invalid");
                return Result<RefreshTokenResponse>.Error("Refresh token is invalid or not found");
            }

            // Check if user has this refresh token and if it's valid
            var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);

            if (refreshToken is null || !refreshToken.IsValid)
            {
                _logger.LogWarning("Refresh token is invalid, expired, or revoked for user {UserId}", user.Id.Value);
                return Result<RefreshTokenResponse>.Error("Refresh token is invalid, expired, or revoked");
            }

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(user);

            // Generate new refresh token
            var newRefreshTokenString = _tokenService.GenerateRefreshToken(user.Id.Value);
            var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
            var newRefreshToken = Auth.Domain.ValueObjects.RefreshToken.Create(newRefreshTokenString, newRefreshTokenExpiresAt);

            // Revoke old refresh token
            refreshToken.RevokedAt = DateTime.UtcNow;

            // Add new refresh token to user
            user.AddRefreshToken(newRefreshToken);

            // Persist changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Token refreshed successfully for user {UserId}",
                user.Id.Value);

            var response = new RefreshTokenResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshTokenString,
                ExpiresInSeconds: _jwtSettings.TokenExpiryInMinutes * 60);

            return Result<RefreshTokenResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<RefreshTokenResponse>.Error("An error occurred while refreshing the token");
        }
    }
}
