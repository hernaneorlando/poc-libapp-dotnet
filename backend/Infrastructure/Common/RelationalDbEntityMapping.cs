using Application.Common.BaseDTO;
using Domain.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Common;

internal static class RelationalDbEntityMapping
{
    internal static void ConvertEntityBaseProperties(this BaseDto dto, RelationalDbBaseBaseEntity entity)
    {
        dto.Id = entity.ExternalId;
        dto.CreatedAt = entity.CreatedAt;
        dto.UpdatedAt = entity.UpdatedAt;
        dto.Active = entity.Active;
    }

    internal static void ConvertModelBaseProperties<TModel>(this RelationalDbBaseBaseEntity entity, RelationalDbModel<TModel> model)
        where TModel : RelationalDbModel<TModel>
    {
        entity.Id = model.Id;
        entity.ExternalId = model.ExternalId;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.Active = model.Active;
    }

    internal static void ConvertEntityBaseProperties<TModel>(this TModel model, RelationalDbBaseBaseEntity entity)
        where TModel : RelationalDbModel<TModel>
    {
        model.Id = entity.Id;
        model.ExternalId = entity.ExternalId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        model.Active = entity.Active;
    }
}
