using Application.ReportManagement.AuditEntries.Services;
using Domain.ReportManagement;
using FluentResults;
using Infrastructure.Persistence.Context;
using MongoDB.Driver;

namespace Infrastructure.Services.ReportManagement;

public class AuditEntryService(NoSqlDataContext noSqlDataContext) : IAuditEntryService
{
    public async Task<Result<IEnumerable<AuditEntry>>> GetAuditEntriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var auditEntries = await noSqlDataContext.AuditEntries
            .Find(entry => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        if (auditEntries is null || auditEntries.Count == 0)
        {
            return Result.Fail("No audit entries found.");
        }

        return Result.Ok(auditEntries.Select(permission => (AuditEntry)permission));
    }
}
