namespace LibraryApp.API.Endpoints.Auth;

using global::Auth.Application.Users.Commands.CreateUser;
using global::Auth.Application.Users.Commands.AddDeniedPermission;
using global::Auth.Application.Users.Commands.RemoveDeniedPermission;
using global::Auth.Application.Users.Queries.GetDeniedPermissions;
using global::Auth.Application.Users.DTOs;
using global::Auth.Domain.Attributes;
using global::Auth.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// User management endpoints (minimal API).
/// Handles user creation.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Registers all user-related endpoints to the application.
    /// </summary>
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        var userGroupBuilder = app.MapGroup("api/auth/users")
            .WithGroupName("User Management")
            .WithTags("Users");

        // POST /api/auth/users - Create a new user
        userGroupBuilder.MapPost("", CreateUser)
            .WithName(nameof(CreateUser))
            .WithSummary("Create a new user account")
            .WithDescription("Creates a new user account with the provided information. Username is auto-generated.")
            .Produces<UserDTO>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        // POST /api/auth/users/{userId}/denied-permissions - Add denied permission
        userGroupBuilder.MapPost("{userId}/denied-permissions", AddDeniedPermission)
            .WithName(nameof(AddDeniedPermission))
            .WithSummary("Add a denied permission for a user")
            .WithDescription("Adds an exception permission that denies a user access to a specific resource action.")
            .Produces<ErrorResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        // DELETE /api/auth/users/{userId}/denied-permissions - Remove denied permission
        userGroupBuilder.MapDelete("{userId}/denied-permissions", RemoveDeniedPermission)
            .WithName(nameof(RemoveDeniedPermission))
            .WithSummary("Remove a denied permission for a user")
            .WithDescription("Removes a denied permission exception, allowing the user to access the resource action if granted through roles.")
            .Produces<ErrorResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        // GET /api/auth/users/{userId}/denied-permissions - Get denied permissions
        userGroupBuilder.MapGet("{userId}/denied-permissions", GetDeniedPermissions)
            .WithName(nameof(GetDeniedPermissions))
            .WithSummary("Get all denied permissions for a user")
            .WithDescription("Retrieves the list of all denied permissions for a specific user.")
            .Produces<GetUserDeniedPermissionsResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Creates a new user account.
    /// Validates input, generates username, creates aggregate, and persists to database.
    /// Can optionally assign roles during user creation.
    /// Requires: User.Create permission
    /// </summary>
    [RequirePermission(PermissionFeature.User, PermissionAction.Create)]
    private static async Task<IResult> CreateUser(
        [FromServices] IMediator mediator,
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        // Map request to command with optional roles
        var command = new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.UserType,
            request.PhoneNumber,
            request.RoleIds
        );

        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: userDto => Results.CreatedAtRoute("CreateUser", null, userDto),
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "User Creation Failed",
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

    /// <summary>
    /// Adds a denied permission for a user.
    /// A denied permission explicitly prevents access to a resource action, overriding role permissions.
    /// Requires: User.Update permission
    /// </summary>
    [RequirePermission(PermissionFeature.User, PermissionAction.Update)]
    private static async Task<IResult> AddDeniedPermission(
        [FromServices] IMediator mediator,
        [FromRoute] string userId,
        [FromBody] DeniedPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddDeniedPermissionCommand(userId, request.Feature, request.Action);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            onSuccess: response => Results.Ok(response),
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "Failed to Add Denied Permission",
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

    /// <summary>
    /// Removes a denied permission for a user.
    /// Allows the user to access the resource action if granted through their roles.
    /// Requires: User.Update permission
    /// </summary>
    [RequirePermission(PermissionFeature.User, PermissionAction.Update)]
    private static async Task<IResult> RemoveDeniedPermission(
        [FromServices] IMediator mediator,
        [FromRoute] string userId,
        [FromBody] DeniedPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RemoveDeniedPermissionCommand(userId, request.Feature, request.Action);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            onSuccess: response => Results.Ok(response),
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "Failed to Remove Denied Permission",
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

    /// <summary>
    /// Retrieves all denied permissions for a user.
    /// Requires: User.Read permission
    /// </summary>
    [RequirePermission(PermissionFeature.User, PermissionAction.Read)]
    private static async Task<IResult> GetDeniedPermissions(
        [FromServices] IMediator mediator,
        [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserDeniedPermissionsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            onSuccess: response => Results.Ok(response),
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "Failed to Retrieve Denied Permissions",
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
/// Request model for managing denied permissions.
/// </summary>
public sealed record DeniedPermissionRequest(
    /// <summary>The permission feature (e.g., Book, Category, User, etc.)</summary>
    string Feature,

    /// <summary>The permission action (Create, Read, Update, Delete)</summary>
    string Action);

/// <summary>
/// Request model for creating a new user.
/// RoleIds are optional and can be used to assign roles during user creation.
/// </summary>
public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string UserType,
    string? PhoneNumber = null,
    IReadOnlyList<string>? RoleIds = null);

/// <summary>
/// Standard error response model following ProblemDetails pattern.
/// </summary>
public sealed record ErrorResponse
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public int Status { get; init; }
    public List<string> Errors { get; init; } = [];
}
