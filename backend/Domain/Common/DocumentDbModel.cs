using Domain.Common.Interfaces;

namespace Domain.Common;

public abstract class DocumentDbModel<TModel> : DbBaseModel<TModel>
    where TModel : DbBaseModel<TModel>
{
    public string Id { get; set; } = string.Empty;
}