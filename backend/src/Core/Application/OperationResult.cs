namespace Core.Application;

/// <summary>
/// Base class for using FluentResults or Result Pattern.
/// This is a simple abstraction to standardize operation returns.
/// Contexts can use FluentResults.Result directly or inherit from this.
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; protected set; }
    public List<string> Errors { get; protected set; } = [];
    public DateTime ExecutedAt { get; protected set; } = DateTime.UtcNow;

    protected OperationResult(bool isSuccess = false)
    {
        IsSuccess = isSuccess;
    }

    public static OperationResult Success() => new(true);
    public static OperationResult Failure(string error) => new(false) { Errors = [error] };
    public static OperationResult Failure(IEnumerable<string> errors) => new(false) { Errors = [..errors] };
}

/// <summary>
/// Typed result with a return value.
/// </summary>
public class OperationResult<T> : OperationResult
{
    public T? Value { get; set; }

    private OperationResult(bool isSuccess = false) : base(isSuccess) { }

    public static OperationResult<T> Success(T value) => new(true) { Value = value };
    public new static OperationResult<T> Failure(string error) => new(false) { Errors = [error] };
    public new static OperationResult<T> Failure(IEnumerable<string> errors) => new(false) { Errors = [..errors] };
}
