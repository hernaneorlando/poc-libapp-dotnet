using Application.Common.BaseDTO;
using Application.ReportManagement.AuditEntries.DTOs;
using Application.ReportManagement.AuditEntries.Services;
using Domain.Common;
using MediatR;

namespace Application.ReportManagement.AuditEntries.Queries;

public class GetAuditEntriesHandler(IAuditEntryService auditEntryService) : IRequestHandler<GetAuditEntriesQuery, ValidationResult<PagedResponseDTO<AuditEntryDTO>>>
{
    public async Task<ValidationResult<PagedResponseDTO<AuditEntryDTO>>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var auditEntryResults = await auditEntryService.GetAuditEntriesAsync(request.PageNumber, request.PageSize, cancellationToken);
        return auditEntryResults.IsSuccess
            ? ValidationResult.Ok(new PagedResponseDTO<AuditEntryDTO> { Data = [..auditEntryResults.Value.Select(auditEntry => (AuditEntryDTO)auditEntry)] })
            : ValidationResult.Fail<PagedResponseDTO<AuditEntryDTO>>(auditEntryResults.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve audit entries.");
    }
}