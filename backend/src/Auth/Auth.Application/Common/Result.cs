namespace Auth.Application.Common;

/// <summary>
/// Generic result wrapper for command/query responses.
/// Supports success, error, and validation states.
/// </summary>
public abstract record Result<T>
{
    public sealed record Success(T Data) : Result<T>;
    public sealed record ErrorState(string Message) : Result<T>;
    public sealed record ValidationError(IReadOnlyList<string> Errors) : Result<T>;

    /// <summary>
    /// Factory method to create a successful result.
    /// </summary>
    public static Result<T> Ok(T data) => new Success(data);

    /// <summary>
    /// Factory method to create an error result.
    /// </summary>
    public static Result<T> Error(string message) => new ErrorState(message);

    /// <summary>
    /// Factory method to create a validation error result.
    /// </summary>
    public static Result<T> Invalid(IEnumerable<string> errors) => new ValidationError(errors.ToList());

    /// <summary>
    /// Executes the appropriate delegate based on the result state.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onError,
        Func<IReadOnlyList<string>, TResult> onValidationError) =>
        this switch
        {
            Success success => onSuccess(success.Data),
            ErrorState error => onError(error.Message),
            ValidationError validationError => onValidationError(validationError.Errors),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    /// <summary>
    /// Executes the appropriate action based on the result state.
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<string> onError,
        Action<IReadOnlyList<string>> onValidationError)
    {
        switch (this)
        {
            case Success success:
                onSuccess(success.Data);
                break;
            case ErrorState error:
                onError(error.Message);
                break;
            case ValidationError validationError:
                onValidationError(validationError.Errors);
                break;
        }
    }
}
