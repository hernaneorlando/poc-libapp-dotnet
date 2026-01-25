namespace Auth.Application.Services;

using Auth.Domain.Services;
using Auth.Infrastructure.Repositories.Interfaces;
using Auth.Infrastructure.Specifications;

/// <summary>
/// Implementation of IUsernameGeneratorService.
/// Generates unique usernames with automatic fallback to numeric suffixes.
/// </summary>
public sealed class UsernameGeneratorService(IUserRepository _userRepository) : IUsernameGeneratorService
{
    private const int MaxAlternativesToSuggest = 5;

    /// <summary>
    /// Generates a username from first and last names.
    /// If the primary username exists, returns alternatives with numeric suffixes.
    /// </summary>
    public async Task<UsernameGenerationResult> GenerateUsernameAsync(string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First name and last name cannot be empty");

        // Generate primary username
        var primaryUsername = Username.GenerateFromName(firstName, lastName);

        // Check if available
        var existingUser = await _userRepository.FindAsync(new GetUserByUsername(primaryUsername.Value), cancellationToken);
        if (existingUser is null)
            return UsernameGenerationResult.Success(primaryUsername);

        // Primary username taken, generate alternatives with numeric suffixes
        var alternatives = new List<Username>();
        for (int i = 1; i <= MaxAlternativesToSuggest; i++)
        {
            var candidateUsername = Username.GenerateFromNameWithSuffix(firstName, lastName, i);
            var exists = await _userRepository.FindAsync(new GetUserByUsername(candidateUsername.Value), cancellationToken);
            if (exists is null)
            {
                alternatives.Add(candidateUsername);
            }
        }

        if (alternatives.Count == 0)
            throw new InvalidOperationException(
                $"Unable to generate available username for {firstName} {lastName}. Please try a custom username.");

        return UsernameGenerationResult.WithAlternatives(primaryUsername, alternatives);
    }

    /// <summary>
    /// Validates a custom username and checks for uniqueness.
    /// </summary>
    public async Task<bool> IsUsernameAvailableAsync(Username username, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);

        var existingUser = await _userRepository.FindAsync(new GetUserByUsername(username.Value), cancellationToken);
        return existingUser is null;
    }
}
