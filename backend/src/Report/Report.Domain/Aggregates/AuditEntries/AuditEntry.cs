using Report.Domain.Enums;
using Report.Domain.ValueObjects;

namespace Report.Domain.Aggregates.AuditEntries;

public class AuditEntry
{
    public string Id { get; set; } = string.Empty;
    public required Guid ExternalId { get; set; }
    public DateTime Timestamp { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public AuditActionEnum Action { get; set; }
    public required UserId UserId { get; set; }
    public IList<FieldChange> Changes { get; set; } = [];
}
