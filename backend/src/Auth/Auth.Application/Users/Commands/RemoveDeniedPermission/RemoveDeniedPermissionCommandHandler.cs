namespace Auth.Application.Users.Commands.RemoveDeniedPermission;

using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.Repositories;
using Auth.Domain;
using Auth.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for RemoveDeniedPermissionCommand.
/// Removes a permission from the user's denied permissions list.
/// </summary>
public sealed class RemoveDeniedPermissionCommandHandler(
    IUserRepository _userRepository,
    IUnitOfWork _unitOfWork,
    ILogger<RemoveDeniedPermissionCommandHandler> _logger) : IRequestHandler<RemoveDeniedPermissionCommand, Result<RemoveDeniedPermissionResponse>>
{
    public async Task<Result<RemoveDeniedPermissionResponse>> Handle(
        RemoveDeniedPermissionCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Removing denied permission {Feature}:{Action} for user {UserId}",
            request.Feature,
            request.Action,
            request.UserId);

        // Validate command
        var validator = new RemoveDeniedPermissionCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new Core.Validation.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        // Parse user ID
        if (!Guid.TryParse(request.UserId, out var userGuid))
            throw new InvalidOperationException("Invalid user ID format");

        var userId = UserId.From(userGuid);

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found");

        // Parse feature and action
        if (!Enum.TryParse<PermissionFeature>(request.Feature, ignoreCase: true, out var feature))
            throw new InvalidOperationException($"Invalid feature: {request.Feature}");

        if (!Enum.TryParse<PermissionAction>(request.Action, ignoreCase: true, out var action))
            throw new InvalidOperationException($"Invalid action: {request.Action}");

        // Create permission
        var permission = new Permission(feature, action);

        // Check if permission is in denied list
        if (!user.DeniedPermissions.Contains(permission))
            throw new InvalidOperationException($"Permission {feature}:{action} is not in the denied list for this user");

        // Remove from denied permissions
        user.DeniedPermissions.Remove(permission);

        // Persist changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully removed denied permission {Feature}:{Action} for user {UserId}",
            feature,
            action,
            userGuid);

        var response = new RemoveDeniedPermissionResponse(
            UserId: userGuid.ToString(),
            Feature: feature.ToString(),
            Action: action.ToString(),
            Message: $"Permission {feature}:{action} has been removed from denied list for user");

        return Result<RemoveDeniedPermissionResponse>.Ok(response);
    }
}
