using Domain.Common;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Extension;

public static class ResultExtensions
{
    public static IActionResult Match<TValue>(
        this ValidationResult<TValue> result,
        Func<TValue, IActionResult> onSuccess,
        Func<IEnumerable<string>, IActionResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Errors);
    }
    
    public static IActionResult Match(
        this ValidationResult result,
        Func<IActionResult> onSuccess,
        Func<IEnumerable<string>, IActionResult> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess()
            : onFailure(result.Errors);
    }

    [Obsolete("To remove FluentResult dependency")]
    public static IActionResult Match<TValue>(
        this Result<TValue> result,
        Func<TValue, IActionResult> onSuccess,
        Func<IEnumerable<IError>, IActionResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Errors);
    }

    [Obsolete("To remove FluentResult dependency")]
    public static IActionResult Match(
        this Result result,
        Func<IActionResult> onSuccess,
        Func<IEnumerable<IError>, IActionResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }
}