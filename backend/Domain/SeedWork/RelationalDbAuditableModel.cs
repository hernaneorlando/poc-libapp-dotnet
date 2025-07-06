using System;

namespace Domain.SeedWork;

public class RelationalDbAuditableModel : IAuditableModel
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
