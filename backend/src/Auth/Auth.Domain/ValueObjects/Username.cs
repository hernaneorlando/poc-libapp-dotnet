namespace Auth.Domain.ValueObjects;

/// <summary>
/// Value Object representing a unique username.
/// Usernames must be unique across the system and follow validation rules.
/// </summary>
public sealed partial class Username : ValueObject
{
    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-z0-9._-]+$")]
    private static partial System.Text.RegularExpressions.Regex ValidUsernameFormatRegex();

    public string Value { get; set; } = string.Empty;

    public Username() { }

    /// <summary>
    /// Creates a new username with validation.
    /// </summary>
    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty");

        var trimmedValue = value.Trim().ToLowerInvariant();

        if (trimmedValue.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters");

        if (trimmedValue.Length > 50)
            throw new ArgumentException("Username must not exceed 50 characters");

        if (!IsValidUsernameFormat(trimmedValue))
            throw new ArgumentException("Username contains invalid characters. Use only letters, numbers, dots, hyphens, and underscores");

        return new Username { Value = trimmedValue };
    }

    /// <summary>
    /// Generates a username from first and last names.
    /// Format: firstname.lastname (both lowercased)
    /// </summary>
    public static Username GenerateFromName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First name and last name cannot be empty");

        var generated = $"{firstName.ToLower()}.{lastName.ToLower()}";
        return Create(generated);
    }

    /// <summary>
    /// Generates a username with a numeric suffix to ensure uniqueness.
    /// Format: firstname.lastnameN (e.g., john.doe1, john.doe2)
    /// </summary>
    public static Username GenerateFromNameWithSuffix(string firstName, string lastName, int suffix)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First name and last name cannot be empty");

        if (suffix <= 0)
            throw new ArgumentException("Suffix must be greater than 0");

        var generated = $"{firstName.ToLower()}.{lastName.ToLower()}{suffix}";
        return Create(generated);
    }

    /// <summary>
    /// Validates the username format.
    /// Allowed characters: a-z, 0-9, dots (.), hyphens (-), underscores (_)
    /// </summary>
    private static bool IsValidUsernameFormat(string value)
    {
        return ValidUsernameFormatRegex().IsMatch(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;    
}
