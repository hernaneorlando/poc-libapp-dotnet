using Application.ReportManagement.AuditEntries.DTOs;
using Application.SeedWork.FluentValidation;

namespace Application.ReportManagement.AuditEntries.Queries;

public class GetAuditEntriesValidator : BasePagedQueryValidator<GetAuditEntriesQuery, AuditEntryDTO>;