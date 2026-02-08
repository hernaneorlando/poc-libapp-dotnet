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

        // Generate new refresh token
        var newRefreshTokenString = _tokenService.GenerateRefreshToken(user.Id.Value);
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
        var newRefreshToken = RefreshToken.Create(newRefreshTokenString, newRefreshTokenExpiresAt);

        // Revoke old refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Add new refresh token to user
        user.AddRefreshToken(newRefreshToken);

        // Persist changes
        await _userRepository.UpdateAsync(user, cancellationToken);
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
}
