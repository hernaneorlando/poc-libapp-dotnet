using Domain.Common.Interfaces;
using Domain.Common.Util;

namespace Domain.Common;

public abstract class RelationalDbModel<TModel> : DbBaseModel<TModel>
    where TModel : DbBaseModel<TModel>
{
    public long Id { get; set; }
}