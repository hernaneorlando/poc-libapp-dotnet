namespace Domain.Common;

public class ValidationResult
{
    protected readonly List<string> _errors = [];

    public bool IsSuccess => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string message) => _errors.Add(message);

    public void AddErrors(IEnumerable<string> messages) => _errors.AddRange(messages);

    public static ValidationResult<TValue> Create<TValue>()
    {
        return new ValidationResult<TValue>();
    }

    public static ValidationResult<TValue> Create<TValue>(IEnumerable<string> errors)
    {
        var result = new ValidationResult<TValue>();
        if (errors.Any())
            result.AddErrors(errors);

        return result;
    }

    public static ValidationResult<TValue> Ok<TValue>(TValue value)
    {
        var result = new ValidationResult<TValue>();
        result.AddValue(value);
        return result;
    }

    public static ValidationResult<TValue> Fail<TValue>(string error) => Fail<TValue>([error]);

    public static ValidationResult<TValue> Fail<TValue>(IEnumerable<string> errors)
    {
        var result = new ValidationResult<TValue>();
        result.AddErrors(errors);
        return result;
    }
}

public class ValidationResult<TValue> : ValidationResult
{

    public TValue Value { get; private set; }

    public ValidationResult() => Value = default!;

    public void AddValue(TValue value) => Value = value;
}