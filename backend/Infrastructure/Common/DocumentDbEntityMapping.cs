using Domain.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Common;

public static class DocumentDbEntityMapping
{
    public static void ConvertModelBaseProperties<TModel>(this DocumentDbEntity entity, DocumentDbModel<TModel> model)
        where TModel : DbBaseModel<TModel>
    {
        entity.Id = model.Id;
        entity.ExternalId = model.ExternalId;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.Active = model.Active;
    }

    public static void ConvertEntityBaseProperties<TModel>(this DocumentDbModel<TModel> model, DocumentDbEntity entity)
        where TModel : DbBaseModel<TModel>
    {
        model.Id = entity.Id;
        model.ExternalId = entity.ExternalId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        model.Active = entity.Active;
    }
}
