using Domain.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Common;

public static class DocumentDbEntityMapping
{
    public static void ConvertModelBaseProperties(this DocumentDbEntity entity, DocumentDbModel model)
    {
        entity.Id = model.Id;
        entity.ExternalId = model.ExternalId;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.Active = model.Active;
    }

    public static void ConvertEntityBaseProperties(this DocumentDbModel model, DocumentDbEntity entity)
    {
        model.Id = entity.Id;
        model.ExternalId = entity.ExternalId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        model.Active = entity.Active;
    }
}
