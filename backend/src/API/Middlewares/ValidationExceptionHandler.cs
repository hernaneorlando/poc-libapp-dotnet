using Core.Validation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace LibraryApp.API.Middlewares;

/// <summary>
/// Global exception handler for validation and application exceptions.
/// </summary>
public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        return exception switch
        {
            ValidationException validationException => await HandleValidationExceptionAsync(
                httpContext, validationException, cancellationToken),
            InvalidOperationException invalidOpException => await HandleInvalidOperationExceptionAsync(
                httpContext, invalidOpException, cancellationToken),
            _ => HandleUnhandledException(exception)
        };
    }

    private async Task<bool> HandleValidationExceptionAsync(
        HttpContext httpContext,
        ValidationException exception,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Validation exception occurred");

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var error = new ResultError(
            Title: "Validation Error",
            Details: exception.Errors.Any()
                ? string.Join($", {Environment.NewLine}", exception.Errors)
                : exception.Message,
            StatusCode: StatusCodes.Status400BadRequest
        );

        await WriteResponseAsync(httpContext, error, cancellationToken);
        return true;
    }

    private async Task<bool> HandleInvalidOperationExceptionAsync(
        HttpContext httpContext,
        InvalidOperationException exception,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Invalid operation exception occurred");

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var error = new ResultError(
            Title: "Invalid Operation",
            Details: exception.Message,
            StatusCode: StatusCodes.Status400BadRequest
        );

        await WriteResponseAsync(httpContext, error, cancellationToken);
        return true;
    }

    private bool HandleUnhandledException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");
        return false;
    }

    private static async Task WriteResponseAsync(
        HttpContext httpContext,
        ResultError error,
        CancellationToken cancellationToken)
    {
        var jsonResponse = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);
    }
}
