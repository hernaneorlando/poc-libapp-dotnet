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
            return Result<RemoveDeniedPermissionResponse>.Invalid(
                validationResult.Errors.Select(e => e.ErrorMessage));

        try
        {
            // Parse user ID
            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", request.UserId);
                return Result<RemoveDeniedPermissionResponse>.Error("Invalid user ID format");
            }

            var userId = UserId.From(userGuid);

            // Get user
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userGuid);
                return Result<RemoveDeniedPermissionResponse>.Error("User not found");
            }

            // Parse feature and action
            if (!Enum.TryParse<PermissionFeature>(request.Feature, ignoreCase: true, out var feature))
            {
                _logger.LogWarning("Invalid feature: {Feature}", request.Feature);
                return Result<RemoveDeniedPermissionResponse>.Error($"Invalid feature: {request.Feature}");
            }

            if (!Enum.TryParse<PermissionAction>(request.Action, ignoreCase: true, out var action))
            {
                _logger.LogWarning("Invalid action: {Action}", request.Action);
                return Result<RemoveDeniedPermissionResponse>.Error($"Invalid action: {request.Action}");
            }

            // Create permission
            var permission = new Permission(feature, action);

            // Check if permission is in denied list
            if (!user.DeniedPermissions.Contains(permission))
            {
                _logger.LogWarning(
                    "Permission {Feature}:{Action} not in denied list for user {UserId}",
                    feature,
                    action,
                    userGuid);
                return Result<RemoveDeniedPermissionResponse>.Error(
                    $"Permission {feature}:{action} is not in the denied list for this user");
            }

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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error removing denied permission {Feature}:{Action} for user {UserId}",
                request.Feature,
                request.Action,
                request.UserId);
            return Result<RemoveDeniedPermissionResponse>.Error("An error occurred while removing denied permission");
        }
    }
}
