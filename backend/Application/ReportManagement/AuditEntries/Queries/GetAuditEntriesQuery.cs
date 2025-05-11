using Application.ReportManagement.AuditEntries.DTOs;
using Application.SeedWork.MediatR;

namespace Application.ReportManagement.AuditEntries.Queries;

public record GetAuditEntriesQuery : BasePagedQuery<AuditEntryDTO>;