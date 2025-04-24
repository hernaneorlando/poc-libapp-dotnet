using Domain.Shared;

namespace Domain.CatalogManagement;

public class Publisher : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}