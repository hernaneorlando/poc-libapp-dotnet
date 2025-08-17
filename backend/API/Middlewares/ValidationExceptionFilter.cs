using LibraryApp.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryApp.API.Middlewares;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var error = new ResultError(
            Title: "Validation Error",
            Details: context.Exception.Message,
            StatusCode: StatusCodes.Status400BadRequest
        );

        context.Result = new BadRequestObjectResult(error);
        context.ExceptionHandled = true;
    }
}