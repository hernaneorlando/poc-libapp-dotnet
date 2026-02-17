namespace Auth.Application.Users.Commands.RefreshToken;

using MediatR;
using Auth.Domain;
using Microsoft.Extensions.Logging;
using Core.API;
using Auth.Application.Common.Security;
using Auth.Infrastructure.Repositories.Interfaces;
using Auth.Application.Common.Security.Interfaces;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Specifications;

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

        // Find user by refresh token
        var user = await _userRepository.FindAsync(new GetUserByRefreshToken(request.RefreshToken), cancellationToken) 
            ?? throw new InvalidOperationException("Refresh token is invalid or not found");

        // Check if user has this refresh token and if it's valid
        var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
        if (refreshToken is null || !refreshToken.IsValid)
        {
            _logger.LogWarning("Refresh token is invalid, expired, or revoked for user {UserId}", user.Id.Value);
            throw new InvalidOperationException("Refresh token is invalid, expired, or revoked");
        }

        // Generate new access token
        var newAccessToken = _tokenService.GenerateAccessToken(user);

        // Use IsRememberMe flag to determine token expiry
        // RememberMe=true: long-lived token, set expiry in days
        // RememberMe=false: sliding window, set expiry in minutes
        var now = DateTime.UtcNow;
        var newTokenExpiresAt = refreshToken.IsRememberMe
            ? now.AddDays(_jwtSettings.RefreshTokenExpiryInDays)
            : now.AddMinutes(_jwtSettings.RefreshTokenSlidingExpiryInMinutes);

        // Revoke the old refresh token
        refreshToken.Revoke();

        // Generate new refresh token
        var newRefreshTokenString = _tokenService.GenerateRefreshToken(user.Id.Value);

        // Create new refresh token and add to user
        // Preserve the IsRememberMe flag from the original token
        var newRefreshToken = RefreshToken.Create(newRefreshTokenString, newTokenExpiresAt, refreshToken.IsRememberMe);
        user.RefreshTokens.Add(newRefreshToken);

        // Persist changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Token refreshed successfully for user {UserId}",
            user.Id.Value);

        var response = new RefreshTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenString);

        return Result<RefreshTokenResponse>.Ok(response);
    }
}
