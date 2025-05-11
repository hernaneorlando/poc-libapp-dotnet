using Application.SeedWork.BaseDTO;
using Domain.SeedWork;

namespace Application.Common;

public static class AuditableDtoMapping
{
    public static void ConvertDtoAuditableProperties(this RelationalDbAuditableModel model, AuditableDto dto)
    {
        model.CreatedAt = dto.CreatedAt;
        model.UpdatedAt = dto.UpdatedAt;
    }

    public static void ConvertModelAuditableProperties(this AuditableDto dto, RelationalDbAuditableModel model)
    {
        dto.CreatedAt = model.CreatedAt;
        dto.UpdatedAt = model.UpdatedAt;
    }
}
