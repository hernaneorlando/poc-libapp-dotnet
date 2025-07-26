using Domain.Common;

namespace Application.Common.BaseDTO;

public abstract record BaseDto : AuditableDto
{
    public Guid Id { get; set; }
    public bool Active { get; set; }

    public void ConvertBaseProperties<TModel>(DocumentDbModel<TModel> model)
        where TModel : DbBaseModel<TModel>
    {
        Id = model.ExternalId;
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
        Active = model.Active;
    }

    public void ConvertBaseProperties<TModel>(RelationalDbModel<TModel> model)
        where TModel : RelationalDbModel<TModel>
    {
        Id = model.ExternalId;
        Active = model.Active;
        ConvertAuditableProperties(model);
    }
}