using Domain.Common.Interfaces;

namespace Application.Common.BaseDTO;

public abstract record AuditableDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void ConvertAuditableProperties(IAuditableModel model)
    {
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
    }
}
