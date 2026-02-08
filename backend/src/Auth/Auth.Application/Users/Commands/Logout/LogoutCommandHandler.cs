namespace Auth.Application.Users.Commands.Logout;

using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Infrastructure.Repositories.Interfaces;
using Core.API;
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

        // Step 1: Find user by ID
        var user = await _userRepository.GetByIdAsync(UserId.From(request.UserId), cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Logout failed: User not found - {UserId}", request.UserId);
            throw new InvalidOperationException("User not found");
        }

        // Step 2: Revoke the refresh token
        user.RevokeRefreshToken(request.RefreshToken);

        // Step 3: Update user to persist the changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Step 4: Persist changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logout successful for user: {UserId}", request.UserId);

        // Step 5: Build response
        var response = new LogoutResponse(
            Success: true,
            Message: "Logout successful. Refresh token has been revoked.");

        return Result<LogoutResponse>.Ok(response);
    }
}
