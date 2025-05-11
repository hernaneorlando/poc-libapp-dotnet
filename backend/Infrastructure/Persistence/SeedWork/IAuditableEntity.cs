namespace Infrastructure.Persistence.SeedWork;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    bool Active { get; set; }
}