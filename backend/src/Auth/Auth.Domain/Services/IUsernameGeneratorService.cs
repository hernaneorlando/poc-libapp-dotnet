namespace Auth.Domain.Services;

using Auth.Domain.Aggregates.User;

/// <summary>
/// Result of username generation attempt.
/// Indicates whether the suggested username is available or if customization is needed.
/// </summary>
public sealed class UsernameGenerationResult
{
    public Username SuggestedUsername { get; }
    public bool IsAvailable { get; }
    public List<Username> Alternatives { get; }

    private UsernameGenerationResult(Username suggestedUsername, bool isAvailable, List<Username> alternatives)
    {
        SuggestedUsername = suggestedUsername;
        IsAvailable = isAvailable;
        Alternatives = alternatives;
    }

    /// <summary>
    /// Creates a successful result with an available username.
    /// </summary>
    public static UsernameGenerationResult Success(Username username)
    {
        return new UsernameGenerationResult(username, true, []);
    }

    /// <summary>
    /// Creates a result indicating the username is taken and offers alternatives.
    /// </summary>
    public static UsernameGenerationResult WithAlternatives(Username suggested, List<Username> alternatives)
    {
        return new UsernameGenerationResult(suggested, false, alternatives);
    }
}

/// <summary>
/// Domain Service for generating and validating usernames.
/// Ensures usernames are unique and follow business rules.
/// </summary>
public interface IUsernameGeneratorService
{
    /// <summary>
    /// Generates a username from first and last names.
    /// If the generated username already exists, returns alternatives with numeric suffixes.
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing suggested username and alternatives if needed</returns>
    Task<UsernameGenerationResult> GenerateUsernameAsync(
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a custom username and checks for uniqueness.
    /// </summary>
    /// <param name="username">Custom username to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if username is valid and available; false otherwise</returns>
    Task<bool> IsUsernameAvailableAsync(
        Username username,
        CancellationToken cancellationToken = default);
}
