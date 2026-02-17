using Auth.Application.Users.Commands.Login;
using Auth.Application.Users.Commands.Logout;
using Auth.Application.Users.Commands.RefreshToken;

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
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: loginResponse => Results.Ok(ApiResult<LoginResponse>.Ok(loginResponse)),
            onError: errorMessage => Results.Unauthorized(),
            onValidationError: errors => Results.BadRequest(ApiResult<LoginResponse>.Error(
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
    /// Logs out a user by revoking their refresh token.
    /// Prevents future token refreshes with the revoked token.
    /// </summary>
    private static async Task<IResult> Logout(
        [FromServices] IMediator mediator,
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: logoutResponse => Results.Ok(ApiResult<LogoutResponse>.Ok(logoutResponse)),
            onError: errorMessage => Results.BadRequest(ApiResult<LogoutResponse>.Error(
                new ErrorResponse
                {
                    Title = "Logout Failed",
                    Detail = errorMessage,
                    Status = StatusCodes.Status400BadRequest
                }
            )),
            onValidationError: errors => Results.BadRequest(ApiResult<LogoutResponse>.Error(
                new ErrorResponse
                {
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Errors = [.. errors]
            }))
        );
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// Generates new access and refresh tokens for continued authenticated access.
    /// </summary>
    private static async Task<IResult> RefreshToken(
        [FromServices] IMediator mediator,
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        // Send command through MediatR pipeline
        var result = await mediator.Send(command, cancellationToken);

        // Handle result using pattern matching
        return result.Match(
            onSuccess: refreshResponse => Results.Ok(ApiResult<RefreshTokenResponse>.Ok(refreshResponse)),
            onError: errorMessage => Results.Unauthorized(),
            onValidationError: errors => Results.BadRequest(ApiResult<RefreshTokenResponse>.Error(
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