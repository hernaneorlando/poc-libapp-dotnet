using Application.UserManagement.Users.DTOs;
using Domain.ReportManagement;

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
}