namespace Auth.Application.Users.Commands.CreateUser;

using MediatR;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Services;
using Auth.Domain.Enums;
using Auth.Application.Users.DTOs;
using Microsoft.Extensions.Logging;
using Core.API;
using Auth.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Handler for creating a new user.
/// Auto-generates username from first and last name.
/// Supports assigning roles during user creation.
/// </summary>
public sealed class CreateUserHandler(
    IUserRepository _userRepository,
    IRoleRepository _roleRepository,
    IUsernameGeneratorService _usernameGenerator,
    IUnitOfWork _unitOfWork,
    ILogger<CreateUserHandler> _logger) : IRequestHandler<CreateUserCommand, Result<UserDTO>>
{
    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user: {FirstName} {LastName}", request.FirstName, request.LastName);

        // Validate command
        var validator = new CreateUserCommandValidator(_userRepository);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
            throw new Core.Validation.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        // Generate username from first and last name
        var usernameResult = await _usernameGenerator.GenerateUsernameAsync(request.FirstName, request.LastName, cancellationToken);
        
        if (!usernameResult.IsAvailable)
            throw new InvalidOperationException("Failed to generate a unique username");

        // Parse user type
        if (!Enum.TryParse<UserType>(request.UserType, ignoreCase: true, out var userType))
            throw new InvalidOperationException($"Invalid user type: '{request.UserType}'");

        // Create user aggregate
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            userType,
            usernameResult.SuggestedUsername,
            request.PhoneNumber
        );

        // Assign roles if provided
        if (request.RoleIds is not null && request.RoleIds.Count > 0)
        {
            _logger.LogInformation("Assigning {RoleCount} roles to user", request.RoleIds.Count);
            
            foreach (var roleIdString in request.RoleIds)
            {
                // Parse role ID from string to Guid
                if (!Guid.TryParse(roleIdString, out var roleGuid))
                    throw new InvalidOperationException($"Invalid role ID format: '{roleIdString}'");

                var roleId = Auth.Domain.Aggregates.Role.RoleId.From(roleGuid);

                // Fetch role from repository
                var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
                if (role is null)
                    throw new InvalidOperationException($"Role with ID '{roleIdString}' not found");

                // Assign role to user
                user.AssignRole(role);
                _logger.LogInformation("Assigned role {RoleId} ({RoleName}) to user", role.Id.Value, role.Name);
            }
        }

        // Persist user
        await _userRepository.AddAsync(user, cancellationToken);
        
        // Flush changes to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully: {UserId} ({Username})", user.Id.Value, user.Username.Value);

        // Map to DTO
        var userDto = UserDTO.FromDomain(user);
        return Result<UserDTO>.Ok(userDto);
    }
}
