namespace Auth.Domain;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Responsible for coordinating the work of multiple repositories
/// and ensuring atomic operations across aggregates.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made to tracked entities.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a database transaction wrapper.
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
