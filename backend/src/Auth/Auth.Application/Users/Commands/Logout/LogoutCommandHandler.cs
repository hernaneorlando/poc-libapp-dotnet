namespace Auth.Application.Users.Commands.Logout;

using Auth.Application.Common;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for LogoutCommand.
/// Revokes the user's refresh token, invalidating future refresh attempts.
/// </summary>
public sealed class LogoutCommandHandler(
    IUserRepository _userRepository,
    ILogger<LogoutCommandHandler> _logger,
    IUnitOfWork _unitOfWork) : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
{
    public async Task<Result<LogoutResponse>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout attempt for user: {UserId}", request.UserId);

        try
        {
            // Step 1: Find user by ID
            var user = await _userRepository.GetByIdAsync(UserId.From(request.UserId), cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Logout failed: User not found - {UserId}", request.UserId);
                return Result<LogoutResponse>.Error("User not found");
            }

            // Step 2: Revoke the refresh token
            try
            {
                user.RevokeRefreshToken(request.RefreshToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Logout failed: Token not found for user - {UserId}", request.UserId);
                return Result<LogoutResponse>.Error("Refresh token not found or already revoked");
            }

            // Step 3: Persist changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Logout successful for user: {UserId}", request.UserId);

            // Step 4: Build response
            var response = new LogoutResponse(
                Success: true,
                Message: "Logout successful. Refresh token has been revoked.");

            return Result<LogoutResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", request.UserId);
            return Result<LogoutResponse>.Error("An error occurred during logout");
        }
    }
}
