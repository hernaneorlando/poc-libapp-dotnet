
namespace Domain.Common;

public class ValidationException(string message) : Exception(message)
{
    public ValidationException(IEnumerable<string> errors)
        : this("One or more domain validation errors occurred.")
    {
        Errors = [.. errors];
    }

    public IReadOnlyList<string> Errors { get; } = [];
}