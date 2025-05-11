namespace Infrastructure.Persistence.SeedWork;

public abstract class RelationalDbAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
