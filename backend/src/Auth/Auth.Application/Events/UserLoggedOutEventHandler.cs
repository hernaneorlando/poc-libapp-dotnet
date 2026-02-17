namespace Auth.Application.Events;

using Auth.Domain;
using Auth.Domain.DomainEvents;
using Auth.Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles <see cref="UserLoggedOutEvent"/> by revoking all active refresh tokens
/// for the user (except the current token) and persisting the change.
/// </summary>
public sealed class UserLoggedOutEventHandler(
    IUserRepository _userRepository,
    ILogger<UserLoggedOutEventHandler> _logger,
    IUnitOfWork _unitOfWork) : INotificationHandler<UserLoggedOutEvent>
{   
    public async Task Handle(UserLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserLoggedOutEvent: Revoking all tokens for user {UserId}", notification.UserId);

        var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
            return;

        var tokensToRevoke = user.RefreshTokens
            .Where(rt => !rt.IsRevoked)
            .ToList();

        if (tokensToRevoke.Count == 0)
        {
            _logger.LogDebug("No additional tokens to revoke for user {UserId}", notification.UserId);
            return;
        }

        foreach (var token in tokensToRevoke)
            token.Revoke();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked {Count} additional tokens for user {UserId}", tokensToRevoke.Count, notification.UserId);
    }
}