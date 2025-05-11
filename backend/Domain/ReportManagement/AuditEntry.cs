using Domain.ReportManagement.Enums;
using Domain.ReportManagement.ValueObjects;
using Domain.UserManagement;

namespace Domain.ReportManagement;

public class AuditEntry
{
    public string Id { get; set; } = string.Empty;
    public required Guid ExternalId { get; set; }
    public DateTime Timestamp { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public AuditActionEnum Action { get; set; }  
    public required User User { get; set; }
    public IList<FieldChange> Changes { get; set; } = [];
}