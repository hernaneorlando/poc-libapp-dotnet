namespace Domain.SeedWork;

public interface IAuditableModel
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}