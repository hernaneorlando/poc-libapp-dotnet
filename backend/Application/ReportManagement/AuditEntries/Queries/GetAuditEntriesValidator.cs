using Application.Common.FluentValidation;
using Application.ReportManagement.AuditEntries.DTOs;

namespace Application.ReportManagement.AuditEntries.Queries;

public class GetAuditEntriesValidator : BasePagedQueryValidator<GetAuditEntriesQuery, AuditEntryDTO>;