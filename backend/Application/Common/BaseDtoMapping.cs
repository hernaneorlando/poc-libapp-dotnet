using Application.SeedWork.BaseDTO;
using Domain.SeedWork;

namespace Application.Common;

public static class BaseDtoMapping
{
    public static void ConvertDtoBaseProperties(this RelationalDbBaseModel model, BaseDto dto)
    {
        model.ExternalId = dto.Id;
        model.Active = dto.Active;

        model.ConvertDtoAuditableProperties(dto);
    }

    public static void ConvertModelBaseProperties(this BaseDto dto, RelationalDbBaseModel model)
    {
        dto.Id = model.ExternalId;
        dto.Active = model.Active;

        dto.ConvertModelAuditableProperties(model);
    }

    public static void ConvertDtoBaseProperties(this DocumentDbModel model, BaseDto dto)
    {
        model.ExternalId = dto.Id;
        model.CreatedAt = dto.CreatedAt;
        model.UpdatedAt = dto.UpdatedAt;
        model.Active = dto.Active;
    }

    public static void ConvertModelBaseProperties(this BaseDto dto, DocumentDbModel model)
    {
        dto.Id = model.ExternalId;
        dto.CreatedAt = model.CreatedAt;
        dto.UpdatedAt = model.UpdatedAt;
        dto.Active = model.Active;
    }
}