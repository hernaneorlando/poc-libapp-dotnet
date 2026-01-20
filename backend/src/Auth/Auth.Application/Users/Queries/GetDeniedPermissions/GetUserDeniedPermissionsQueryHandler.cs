namespace Auth.Application.Users.Queries.GetDeniedPermissions;

using Auth.Domain.Aggregates.User;
using Auth.Domain.Repositories;
using Auth.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetUserDeniedPermissionsQuery.
/// Retrieves all denied permissions for a user.
/// </summary>
public sealed class GetUserDeniedPermissionsQueryHandler(
    IUserRepository _userRepository,
    ILogger<GetUserDeniedPermissionsQueryHandler> _logger) : IRequestHandler<GetUserDeniedPermissionsQuery, Result<GetUserDeniedPermissionsResponse>>
{
    public async Task<Result<GetUserDeniedPermissionsResponse>> Handle(
        GetUserDeniedPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving denied permissions for user {UserId}", request.UserId);

        try
        {
            // Parse user ID
            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", request.UserId);
                return Result<GetUserDeniedPermissionsResponse>.Error("Invalid user ID format");
            }

            var userId = UserId.From(userGuid);

            // Get user
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userGuid);
                return Result<GetUserDeniedPermissionsResponse>.Error("User not found");
            }

            // Map denied permissions to DTOs
            var deniedPermissionDtos = user.DeniedPermissions
                .Select(p => new DeniedPermissionDto(
                    Feature: p.Feature.ToString(),
                    Action: p.Action.ToString()))
                .ToList();

            _logger.LogInformation(
                "Successfully retrieved {Count} denied permissions for user {UserId}",
                deniedPermissionDtos.Count,
                userGuid);

            var response = new GetUserDeniedPermissionsResponse(
                UserId: userGuid.ToString(),
                DeniedPermissions: deniedPermissionDtos);

            return Result<GetUserDeniedPermissionsResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving denied permissions for user {UserId}", request.UserId);
            return Result<GetUserDeniedPermissionsResponse>.Error("An error occurred while retrieving denied permissions");
        }
    }
}
