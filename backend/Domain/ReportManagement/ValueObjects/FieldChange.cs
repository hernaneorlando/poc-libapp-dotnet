namespace Domain.ReportManagement.ValueObjects;

public class FieldChange
{
    public required string FieldName { get; set; }
    public string? OldValue { get; set; }
    public required string NewValue { get; set; }
}