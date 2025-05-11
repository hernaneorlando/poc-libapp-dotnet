using Domain.ReportManagement;
using FluentResults;

namespace Application.ReportManagement.AuditEntries.Services;

public interface IAuditEntryService
{
    Task<Result<IEnumerable<AuditEntry>>> GetAuditEntriesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}