using Domain.Common;
using LibraryApp.API.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace LibraryApp.API.Middlewares;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        _logger.LogWarning(validationException, "Validation exception occurred");

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var error = new ResultError(
            Title: "Validation Error",
            Details: validationException.Errors.Any() 
                ? string.Join($",{Environment.NewLine}", validationException.Errors) 
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
