using Application.ReportManagement.AuditEntries.DTOs;
using Application.ReportManagement.AuditEntries.Services;
using FluentResults;
using MediatR;

namespace Application.ReportManagement.AuditEntries.Queries;

public class GetAuditEntriesHandler(IAuditEntryService auditEntryService) : IRequestHandler<GetAuditEntriesQuery, Result<IEnumerable<AuditEntryDTO>>>
{
    public async Task<Result<IEnumerable<AuditEntryDTO>>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var auditEntryResults = await auditEntryService.GetAuditEntriesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return auditEntryResults.IsSuccess
            ? Result.Ok(auditEntryResults.Value.Select(auditEntry => (AuditEntryDTO)auditEntry))
            : Result.Fail<IEnumerable<AuditEntryDTO>>(auditEntryResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve audit entries.");
    }
}