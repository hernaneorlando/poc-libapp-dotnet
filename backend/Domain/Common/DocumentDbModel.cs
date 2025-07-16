using Domain.Common.Interfaces;

namespace Domain.Common;

public abstract class DocumentDbModel : IAuditableModel
{
    public string Id { get; set; } = string.Empty;
    public Guid ExternalId { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
}