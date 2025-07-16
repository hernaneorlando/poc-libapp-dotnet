using Application.Common.MediatR;
using Application.ReportManagement.AuditEntries.DTOs;

namespace Application.ReportManagement.AuditEntries.Queries;

public record GetAuditEntriesQuery : BasePagedQuery<AuditEntryDTO>;