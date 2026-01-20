using Core.Application;

namespace LibraryApp.API.Extensions;

/// <summary>
/// Extension methods for matching operation results with HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Matches operation result to appropriate HTTP response.
    /// </summary>
    public static IResult Match<TValue>(
        this OperationResult<TValue> result,
        Func<TValue, IResult> onSuccess,
        Func<IEnumerable<string>, IResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches operation result to appropriate HTTP response (no value).
    /// </summary>
    public static IResult Match(
        this OperationResult result,
        Func<IResult> onSuccess,
        Func<IEnumerable<string>, IResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }

    /// <summary>
    /// Converts operation result to JSON response with proper status code.
    /// </summary>
    public static IResult ToResponse<TValue>(
        this OperationResult<TValue> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);
        
        var error = new ResultError(
            Title: "Operation Failed",
            Details: string.Join(", ", result.Errors),
            StatusCode: StatusCodes.Status400BadRequest
        );

        return Results.BadRequest(error);
    }

    /// <summary>
    /// Converts operation result to JSON response (no value).
    /// </summary>
    public static IResult ToResponse(
        this OperationResult result)
    {
        if (result.IsSuccess)
            return Results.NoContent();
        
        var error = new ResultError(
            Title: "Operation Failed",
            Details: string.Join(", ", result.Errors),
            StatusCode: StatusCodes.Status400BadRequest
        );

        return Results.BadRequest(error);
    }
}
