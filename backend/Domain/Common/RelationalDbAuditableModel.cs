using System;
using Domain.Common.Interfaces;

namespace Domain.Common;

public class RelationalDbAuditableModel : IAuditableModel
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
