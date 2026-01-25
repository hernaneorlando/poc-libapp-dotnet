using System.Diagnostics.CodeAnalysis;

namespace Core.Validation;

public class ValidationException(string message) : Exception(message)
{
    public ValidationException(IEnumerable<string> errors)
        : this("One or more domain validation errors occurred.")
    {
        Errors = [.. errors];
    }

    public IReadOnlyList<string> Errors { get; } = [];

    public static void ThrowIfNull([NotNull] object? argument, string? message = null)
    {
        if (argument is null)
            throw new ValidationException(message ?? "Argument cannot be null.");
    }

    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ValidationException(message ?? "Argument cannot be null or whitespace.");
    }

    public static void ThrowIfPredicate(bool predicate, string message)
    {
        if (predicate)
            throw new ValidationException(message);
    }
}