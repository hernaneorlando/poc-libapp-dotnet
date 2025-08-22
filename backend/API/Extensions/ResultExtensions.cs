using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Extensions;

public static class ResultExtensions
{
    public static IResult Match<TValue>(
        this ValidationResult<TValue> result,
        Func<TValue, IResult> onSuccess,
        Func<IEnumerable<string>, IResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Errors);
    }

    public static IResult Match(
        this ValidationResult result,
        Func<IResult> onSuccess,
        Func<IEnumerable<string>, IResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }
    
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
}