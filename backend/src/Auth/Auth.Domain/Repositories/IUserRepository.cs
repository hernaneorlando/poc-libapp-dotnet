namespace Auth.Domain.Repositories;

using Auth.Domain.Aggregates.User;

/// <summary>
/// Repository interface for User aggregate persistence.
/// Defines operations for managing users in persistent storage.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by refresh token.
    /// Used for token refresh operations.
    /// </summary>
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from the repository (soft or hard delete).
    /// </summary>
    Task DeleteAsync(UserId userId, CancellationToken cancellationToken = default);
}
