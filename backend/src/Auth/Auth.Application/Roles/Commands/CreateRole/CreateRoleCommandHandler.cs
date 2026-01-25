namespace Auth.Application.Roles.Commands.CreateRole;

using Auth.Application.Roles.DTOs;
using Auth.Domain;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Enums;
using Auth.Infrastructure.Repositories.Interfaces;
using Core.API;

/// <summary>
/// Handler for CreateRoleCommand.
/// Creates a new role and assigns permissions to it.
/// Implements PBAC (Permission-Based Access Control) pattern.
/// </summary>
public sealed class CreateRoleCommandHandler(
    IRoleRepository _roleRepository,
    ILogger<CreateRoleCommandHandler> _logger,
    IUnitOfWork _unitOfWork) : IRequestHandler<CreateRoleCommand, Result<RoleDTO>>
{
    public async Task<Result<RoleDTO>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating role: {RoleName}", request.Name);

        // Step 1: Create role aggregate
        var role = Role.Create(request.Name, request.Description);

        // Step 2: Assign permissions to role
        foreach (var permissionRequest in request.Permissions)
        {
            // Parse feature and action from strings
            if (!Enum.TryParse<PermissionFeature>(
                permissionRequest.Feature, ignoreCase: true, out var feature))
            {
                throw new InvalidOperationException($"Invalid permission feature: {permissionRequest.Feature}");
            }

            if (!Enum.TryParse<PermissionAction>(
                permissionRequest.Action, ignoreCase: true, out var action))
            {
                throw new InvalidOperationException($"Invalid permission action: {permissionRequest.Action}");
            }

            // Create permission value object from feature + action
            var permission = new Permission(feature, action);

            // Assign permission to role
            role.AssignPermission(permission);
        }

        // Step 3: Persist role
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Role created successfully: {RoleId} ({RoleName}) with {PermissionCount} permissions",
            role.Id.Value,
            role.Name,
            role.Permissions.Count);

        // Step 4: Build response
        var response = new RoleDTO(
            Id: role.Id.Value.ToString(),
            Name: role.Name,
            Description: role.Description,
            Permissions: [.. role.Permissions.Select(p => new PermissionDTO(p.Code))],
            CreatedAt: role.CreatedAt,
            UpdatedAt: null,
            IsActive: true);

        return Result<RoleDTO>.Ok(response);
    }
}
