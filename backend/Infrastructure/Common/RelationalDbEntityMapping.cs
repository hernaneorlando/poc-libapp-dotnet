using Domain.SeedWork;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Common;

public static class RelationalDbEntityMapping
{
    public static void ConvertModelBaseProperties(this RelationalDbBaseBaseEntity entity, RelationalDbBaseModel model)
    {
        entity.ExternalId = model.Id;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.Active = model.Active;
    }

    public static void ConvertEntityBaseProperties(this RelationalDbBaseModel model, RelationalDbBaseBaseEntity entity)
    {
        model.Id = entity.ExternalId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        model.Active = entity.Active;
    }
}
