using Domain.Common;

namespace Application.Common.BaseDTO;

public abstract record BaseDto : AuditableDto
{
    public Guid Id { get; set; }
    public bool Active { get; set; }

    public void ConvertBaseProperties(DocumentDbModel model)
    {
        Id = model.ExternalId;
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
        Active = model.Active;
    }

    public void ConvertBaseProperties<TModel>(RelationalDbBaseModel<TModel> model)
        where TModel : RelationalDbBaseModel<TModel>
    {
        Id = model.ExternalId;
        Active = model.Active;
        ConvertAuditableProperties(model);
    }
}