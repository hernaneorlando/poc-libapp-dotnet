namespace Auth.Application.Users.Commands.AddDeniedPermission;

using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.Repositories;
using Auth.Domain;
using Auth.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for AddDeniedPermissionCommand.
/// Adds a permission to the user's denied permissions list.
/// </summary>
public sealed class AddDeniedPermissionCommandHandler(
    IUserRepository _userRepository,
    IUnitOfWork _unitOfWork,
    ILogger<AddDeniedPermissionCommandHandler> _logger) : IRequestHandler<AddDeniedPermissionCommand, Result<AddDeniedPermissionResponse>>
{
    public async Task<Result<AddDeniedPermissionResponse>> Handle(
        AddDeniedPermissionCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding denied permission {Feature}:{Action} for user {UserId}",
            request.Feature,
            request.Action,
            request.UserId);

        // Validate command
        var validator = new AddDeniedPermissionCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result<AddDeniedPermissionResponse>.Invalid(
                validationResult.Errors.Select(e => e.ErrorMessage));

        try
        {
            // Parse user ID
            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", request.UserId);
                return Result<AddDeniedPermissionResponse>.Error("Invalid user ID format");
            }

            var userId = UserId.From(userGuid);

            // Get user
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userGuid);
                return Result<AddDeniedPermissionResponse>.Error("User not found");
            }

            // Parse feature and action
            if (!Enum.TryParse<PermissionFeature>(request.Feature, ignoreCase: true, out var feature))
            {
                _logger.LogWarning("Invalid feature: {Feature}", request.Feature);
                return Result<AddDeniedPermissionResponse>.Error($"Invalid feature: {request.Feature}");
            }

            if (!Enum.TryParse<PermissionAction>(request.Action, ignoreCase: true, out var action))
            {
                _logger.LogWarning("Invalid action: {Action}", request.Action);
                return Result<AddDeniedPermissionResponse>.Error($"Invalid action: {request.Action}");
            }

            // Create permission
            var permission = new Permission(feature, action);

            // Check if already denied
            if (user.DeniedPermissions.Contains(permission))
            {
                _logger.LogWarning(
                    "Permission {Feature}:{Action} already denied for user {UserId}",
                    feature,
                    action,
                    userGuid);
                return Result<AddDeniedPermissionResponse>.Error(
                    $"Permission {feature}:{action} is already denied for this user");
            }

            // Add to denied permissions
            user.DeniedPermissions.Add(permission);

            // Persist changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully added denied permission {Feature}:{Action} for user {UserId}",
                feature,
                action,
                userGuid);

            var response = new AddDeniedPermissionResponse(
                UserId: userGuid.ToString(),
                Feature: feature.ToString(),
                Action: action.ToString(),
                Message: $"Permission {feature}:{action} has been denied for user");

            return Result<AddDeniedPermissionResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding denied permission {Feature}:{Action} for user {UserId}",
                request.Feature,
                request.Action,
                request.UserId);
            return Result<AddDeniedPermissionResponse>.Error("An error occurred while adding denied permission");
        }
    }
}
