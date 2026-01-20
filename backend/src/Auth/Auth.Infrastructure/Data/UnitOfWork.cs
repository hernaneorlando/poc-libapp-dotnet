namespace Auth.Infrastructure.Data;

using Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Implementation of the Unit of Work pattern using Entity Framework Core.
/// Manages database transactions and coordinates persistence operations.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
        }
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new TransactionWrapper(_transaction);
    }

    /// <summary>
    /// Wrapper class for EF Core transactions implementing our ITransaction interface.
    /// </summary>
    private sealed class TransactionWrapper : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public TransactionWrapper(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
        }
    }
}
