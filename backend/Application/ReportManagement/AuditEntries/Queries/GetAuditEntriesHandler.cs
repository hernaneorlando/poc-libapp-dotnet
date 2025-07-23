using Application.ReportManagement.AuditEntries.DTOs;
using Application.ReportManagement.AuditEntries.Services;
using Domain.Common;
using MediatR;

namespace Application.ReportManagement.AuditEntries.Queries;

public class GetAuditEntriesHandler(IAuditEntryService auditEntryService) : IRequestHandler<GetAuditEntriesQuery, ValidationResult<IEnumerable<AuditEntryDTO>>>
{
    public async Task<ValidationResult<IEnumerable<AuditEntryDTO>>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var auditEntryResults = await auditEntryService.GetAuditEntriesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return auditEntryResults.IsSuccess
            ? ValidationResult.Ok(auditEntryResults.Value.Select(auditEntry => (AuditEntryDTO)auditEntry))
            : ValidationResult.Fail<IEnumerable<AuditEntryDTO>>(auditEntryResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve audit entries.");
    }
}