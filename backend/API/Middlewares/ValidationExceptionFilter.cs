using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryApp.API.Middlewares;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = new BadRequestObjectResult(new
        {
            Title = "Validation Error",
            Status = 400,
            Errors = context.Exception.Message,
        });

        context.ExceptionHandled = true;
    }
}