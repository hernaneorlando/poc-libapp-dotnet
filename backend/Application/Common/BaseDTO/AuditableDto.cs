using Domain.Common;

namespace Application.Common.BaseDTO;

public abstract record AuditableDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void ConvertAuditableProperties(RelationalDbAuditableModel model)
    {
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
    }
}
