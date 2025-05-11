namespace Infrastructure.Persistence.SeedWork;

public abstract class RelationalDbBaseBaseEntity : RelationalDbAuditableEntity
{
    public int Id { get; set; }
    public Guid ExternalId { get; set; }
    public bool Active { get; set; }
}