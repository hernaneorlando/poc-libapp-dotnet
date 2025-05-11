namespace Application.SeedWork.BaseDTO;

public abstract record BaseDto : AuditableDto
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}