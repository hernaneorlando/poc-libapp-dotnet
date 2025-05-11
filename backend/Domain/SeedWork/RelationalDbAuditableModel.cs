using System;

namespace Domain.SeedWork;

public class RelationalDbAuditableModel : IAuditableModel
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
