namespace Auth.Application.Users.Commands.Logout;

using Auth.Domain;
using Auth.Domain.DomainEvents;
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
    IUnitOfWork _unitOfWork,
    IMediator _mediator) : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
{
    public async Task<Result<LogoutResponse>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout attempt for user: {ExternalId}", request.ExternalId);

        // Step 1: Find user by External ID
        var user = await _userRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Logout failed: User not found - {ExternalId}", request.ExternalId);
            return Result<LogoutResponse>.Error("User not found");
        }

        // Step 2: Revoke the refresh token (raise domain event to indicate logout)
        user.RevokeRefreshToken(request.RefreshToken);

        // Step 3: Update user to persist the change for the current token
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 4: Publish domain event so handlers (async) can revoke other tokens across devices
        var domainEvent = new UserLoggedOutEvent(user.Id);
        await _mediator.Publish(domainEvent, cancellationToken);

        _logger.LogInformation("Logout successful for user: {ExternalId}", request.ExternalId);

        // Step 5: Build response
        var response = new LogoutResponse(
            Success: true,
            Message: "Logout successful. Refresh token has been revoked.");

        return Result<LogoutResponse>.Ok(response);
    }
}
