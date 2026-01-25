namespace Core.Infrastructure;

public abstract class Entity
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
