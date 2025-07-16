namespace Infrastructure.Persistence.Common;

public abstract class RelationalDbBaseBaseEntity : RelationalDbAuditableEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public bool Active { get; set; }
}