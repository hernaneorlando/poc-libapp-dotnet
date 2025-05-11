namespace Domain.SeedWork;

public abstract class RelationalDbBaseModel : RelationalDbAuditableModel
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}