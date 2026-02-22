using Auth.Application.Roles.Commands.CreateRole;
using Auth.Application.Roles.DTOs;
using Auth.Application.Roles.Queries.ListRoles;

namespace LibraryApp.API.Endpoints.Auth;

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

        // GET /api/auth/roles - List all roles with pagination
        roleGroupBuilder.MapGet("", ListRoles)
            .WithName(nameof(ListRoles))
            .WithSummary("List all roles")
            .WithDescription("Retrieves a paginated list of roles with optional filtering by name and active status.")
            .Produces<PaginatedResponse<RoleDTO>>(StatusCodes.Status200OK)
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
        [FromBody] CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Send command through MediatR pipeline
        var result = await mediator.Send(request, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: roleResponse =>
            {
                var result = ApiResult<RoleDTO>.Ok(roleResponse);
                return Results.CreatedAtRoute("CreateRole", null, result);
            },
            onError: errorMessage => Results.BadRequest(ApiResult<RoleDTO>.Error(
                new ErrorResponse
                {
                    Title = "Role Creation Failed",
                    Detail = errorMessage,
                    Status = StatusCodes.Status400BadRequest
                }
            )),
            onValidationError: errors => Results.BadRequest(ApiResult<RoleDTO>.Error(
                new ErrorResponse
                {
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = [.. errors]
                }
            ))
        );
    }

    /// <summary>
    /// Lists all roles with optional pagination and filtering.
    /// </summary>
    [RequirePermission(PermissionFeature.Role, PermissionAction.Read)]
    private static async Task<IResult> ListRoles(
        [FromServices] IMediator mediator,
        QueryStringWithFilters<ListRolesQuery, RoleDTO> filters,
        CancellationToken cancellationToken = default)
    {
        // Create query from parameters (filters binding is done automatically via QueryStringWithFilters.BindAsync)
        var query = filters.GetQuery();

        // Send query through MediatR pipeline
        var result = await mediator.Send(query, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: listResponse => Results.Ok(ApiResult<PaginatedResponse<RoleDTO>>.Ok(listResponse)),
            onError: errorMessage => Results.BadRequest(ApiResult<PaginatedResponse<RoleDTO>>.Error(
                new ErrorResponse
                {
                    Title = "Failed to Retrieve Roles",
                    Detail = errorMessage,
                    Status = StatusCodes.Status400BadRequest
                }
            )),
            onValidationError: errors => Results.BadRequest(ApiResult<PaginatedResponse<RoleDTO>>.Error(
                new ErrorResponse
                {
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = [.. errors]
                }
            ))
        );
    }
}