using Auth.Application.Users.Commands.Login;
using Auth.Application.Users.Commands.Logout;
using Auth.Application.Users.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Endpoints.Auth;

/// <summary>
/// Authentication endpoints (minimal API).
/// Handles authentication (login/logout).
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registers all authentication-related endpoints to the application.
    /// </summary>
    public static WebApplication AddAuthEndpoints(this WebApplication app)
    {
        var authGroupBuilder = app.MapGroup("api/auth")
            .WithGroupName("Authentication")
            .WithTags("Authentication");

        // POST /api/auth/login - Authenticate user
        authGroupBuilder.MapPost("/login", Login)
            .WithName(nameof(Login))
            .WithSummary("Authenticate user and obtain JWT token")
            .WithDescription("Authenticates a user with username and password, returning JWT access token and refresh token.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // POST /api/auth/logout - Revoke refresh token
        authGroupBuilder.MapPost("/logout", Logout)
            .WithName(nameof(Logout))
            .WithSummary("Logout user by revoking refresh token")
            .WithDescription("Revokes the user's refresh token, preventing future token refreshes.")
            .Produces<LogoutResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // POST /api/auth/refresh - Refresh access token
        authGroupBuilder.MapPost("/refresh", RefreshToken)
            .WithName(nameof(RefreshToken))
            .WithSummary("Refresh access token using refresh token")
            .WithDescription("Generates a new access token and refresh token using a valid refresh token.")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// Returns JWT access token and refresh token for subsequent requests.
    /// </summary>
    private static async Task<IResult> Login(
        [FromServices] IMediator mediator,
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Map request to command
        var command = new LoginCommand(request.Username, request.Password);

        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: loginResponse => Results.Ok(loginResponse),
            onError: errorMessage => Results.Unauthorized(),
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
    /// Logs out a user by revoking their refresh token.
    /// Prevents future token refreshes with the revoked token.
    /// </summary>
    private static async Task<IResult> Logout(
        [FromServices] IMediator mediator,
        [FromBody] LogoutRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Extract user ID from JWT claims
        var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        // Map request to command
        var command = new LogoutCommand(userId, request.RefreshToken);

        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: logoutResponse => Results.Ok(logoutResponse),
            onError: errorMessage => Results.BadRequest(new ErrorResponse
            {
                Title = "Logout Failed",
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
    /// Refreshes the access token using a valid refresh token.
    /// Generates new access and refresh tokens for continued authenticated access.
    /// </summary>
    private static async Task<IResult> RefreshToken(
        [FromServices] IMediator mediator,
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // Map request to command
        var command = new RefreshTokenCommand(request.RefreshToken);

        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: refreshResponse => Results.Ok(refreshResponse),
            onError: errorMessage => Results.Unauthorized(),
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
/// Request model for refreshing an access token.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequest(
    string Username,
    string Password);

/// <summary>
/// Request model for user logout.
/// </summary>
public sealed record LogoutRequest(
    string RefreshToken);