using Core.Validation;
using LibraryApp.API.Extensions;
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
        if (exception is not ValidationException validationException)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
            return false;
        }

        _logger.LogWarning(validationException, "Validation exception occurred");

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var error = new ResultError(
            Title: "Validation Error",
            Details: validationException.Errors.Any()
                ? string.Join($", {Environment.NewLine}", validationException.Errors)
                : validationException.Message,
            StatusCode: StatusCodes.Status400BadRequest
        );

        var jsonResponse = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        return true;
    }
}
