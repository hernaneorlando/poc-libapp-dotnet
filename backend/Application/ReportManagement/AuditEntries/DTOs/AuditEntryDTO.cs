using Application.UserManagement.Users.DTOs;
using Domain.ReportManagement;
using Domain.ReportManagement.Enums;
using Domain.UserManagement;

namespace Application.ReportManagement.AuditEntries.DTOs;

public record AuditEntryDTO(Guid ExternalId, DateTime Timestamp, string EntityName, string EntityId, string Action, UserDto User)
{
    public IList<FieldChangeDTO> Changes { get; set; } = [];

    public static implicit operator AuditEntryDTO(AuditEntry auditEntry)
    {
        return new AuditEntryDTO
        (
            auditEntry.ExternalId,
            auditEntry.Timestamp,
            auditEntry.EntityName,
            auditEntry.EntityId,
            auditEntry.Action.ToString(),
            (UserDto)auditEntry.User
        );
    }

    public static implicit operator AuditEntry(AuditEntryDTO auditEntryDto)
    {
        return new AuditEntry
        {
            ExternalId = auditEntryDto.ExternalId,
            Timestamp = auditEntryDto.Timestamp,
            EntityName = auditEntryDto.EntityName,
            EntityId = auditEntryDto.EntityId,
            Action = Enum.Parse<AuditActionEnum>(auditEntryDto.Action),
            User = (User)auditEntryDto.User
        };
    }
}