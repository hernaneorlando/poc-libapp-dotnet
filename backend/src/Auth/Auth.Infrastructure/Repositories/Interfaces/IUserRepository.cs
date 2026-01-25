namespace Auth.Infrastructure.Repositories.Interfaces;

using Auth.Domain.Aggregates.User;
using Core.Infrastructure;

/// <summary>
/// Repository interface for User aggregate persistence.
/// Defines operations for managing users in persistent storage.
/// </summary>
public interface IUserRepository : IBaseRepository<User, UserEntity>
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
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from the repository (soft or hard delete).
    /// </summary>
    Task DeleteAsync(UserId userId, CancellationToken cancellationToken = default);
}
