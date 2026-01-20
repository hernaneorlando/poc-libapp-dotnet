namespace LibraryApp.API.Endpoints.Auth;

using global::Auth.Application.Roles.Commands.CreateRole;
using global::Auth.Application.Roles.DTOs;
using global::Auth.Domain.Attributes;
using global::Auth.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Role management endpoints (minimal API).
/// Handles role creation with permissions.
/// </summary>
public static class RoleEndpoints
{
    /// <summary>
    /// Registers all role-related endpoints to the application.
    /// </summary>
    public static WebApplication AddRoleEndpoints(this WebApplication app)
    {
        var roleGroupBuilder = app.MapGroup("api/auth/roles")
            .WithGroupName("Role Management")
            .WithTags("Roles");

        // POST /api/auth/roles - Create a new role with permissions
        roleGroupBuilder.MapPost("", CreateRole)
            .WithName(nameof(CreateRole))
            .WithSummary("Create a new role")
            .WithDescription("Creates a new role and assigns permissions to it. Implements PBAC (Permission-Based Access Control) pattern.")
            .Produces<RoleDTO>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        return app;
    }

    /// <summary>
    /// Creates a new role with assigned permissions.
    /// Requires: Role.Create permission
    /// </summary>
    [RequirePermission(PermissionFeature.Role, PermissionAction.Create)]
    private static async Task<IResult> CreateRole(
        [FromServices] IMediator mediator,
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        // Convert API request payloads to command payloads
        var permissions = request.Permissions
            .Select(p => new RolePermissionRequest(p.Feature, p.Action))
            .ToList();

        // Map request to command
        var command = new CreateRoleCommand(
            request.Name,
            request.Description,
            permissions
        );

        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: roleResponse =>
            {
                // Map response to RoleDTO for consistency
                var roleDto = new RoleDTO(
                    Id: roleResponse.Id,
                    Name: roleResponse.Name,
                    Description: roleResponse.Description,
                    Permissions: [.. roleResponse.Permissions.Select(p => new PermissionDTO(p.Feature, p.Action, $"{p.Action}_{p.Feature}"))],
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: null,
                    IsActive: true
                );
                return Results.CreatedAtRoute("CreateRole", null, roleDto);
            },
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "Role Creation Failed",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            }),
            onValidationError: errors => Results.BadRequest(new ErrorResponse
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Errors = [.. errors]
            })
        );
    }
}

/// <summary>
/// Request model for creating a new role.
/// </summary>
public sealed record CreateRoleRequest(
    string Name,
    string Description,
    IReadOnlyList<PermissionRequestPayload> Permissions);

/// <summary>
/// Permission payload in role creation request.
/// </summary>
public sealed record PermissionRequestPayload(
    string Feature,
    string Action);
