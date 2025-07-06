using Domain.SeedWork;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Common;

public static class RelationalDbEntityMapping
{
    public static void ConvertModelBaseProperties(this RelationalDbBaseBaseEntity entity, RelationalDbBaseModel model)
    {
        entity.Id = model.Id;
        entity.ExternalId = model.ExternalId;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.Active = model.Active;
    }

    public static void ConvertEntityBaseProperties(this RelationalDbBaseModel model, RelationalDbBaseBaseEntity entity)
    {
        model.Id = entity.Id;
        model.ExternalId = entity.ExternalId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        model.Active = entity.Active;
    }
}
