using Domain.ReportManagement.ValueObjects;

namespace Application.ReportManagement.AuditEntries.DTOs;

public record FieldChangeDTO(string FieldName, string NewValue)
{
    public string? OldValue { get; set; }

    public static implicit operator FieldChangeDTO(FieldChange fieldChange)
    {
        return new FieldChangeDTO(fieldChange.FieldName, fieldChange.NewValue)
        {
            OldValue = fieldChange.OldValue
        };
    }
}