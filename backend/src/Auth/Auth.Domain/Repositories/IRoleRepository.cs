namespace Auth.Domain.Repositories;

using Auth.Domain.Aggregates.Role;

/// <summary>
/// Repository interface for Role aggregate persistence.
/// Defines operations for managing roles in persistent storage.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Adds a new role to the repository.
    /// </summary>
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by its unique identifier.
    /// </summary>
    Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all roles.
    /// </summary>
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role from the repository (soft or hard delete).
    /// </summary>
    Task DeleteAsync(RoleId roleId, CancellationToken cancellationToken = default);
}
