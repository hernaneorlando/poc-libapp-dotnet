namespace Application.SeedWork.BaseDTO;

public abstract record AuditableDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
