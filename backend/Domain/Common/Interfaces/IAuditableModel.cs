namespace Domain.Common.Interfaces;

public interface IAuditableModel
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}