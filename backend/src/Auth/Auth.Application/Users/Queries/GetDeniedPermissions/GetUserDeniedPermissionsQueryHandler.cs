namespace Auth.Application.Users.Queries.GetDeniedPermissions;

using Auth.Domain.Aggregates.User;
using MediatR;
using Microsoft.Extensions.Logging;
using Core.API;
using Auth.Infrastructure.Repositories.Interfaces;

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

        // Parse user ID
        if (!Guid.TryParse(request.UserId, out var userGuid))
            throw new InvalidOperationException("Invalid user ID format");

        var userId = UserId.From(userGuid);

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found");

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
}
