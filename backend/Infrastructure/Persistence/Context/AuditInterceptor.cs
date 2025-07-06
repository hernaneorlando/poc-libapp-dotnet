using Infrastructure.Persistence.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Context;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetUpdatedTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetUpdatedTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SetUpdatedTimestamps(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Modified && e.Entity is RelationalDbAuditableEntity);

        foreach (var entry in entries)
        {
            ((RelationalDbAuditableEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
