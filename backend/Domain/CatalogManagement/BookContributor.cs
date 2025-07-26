using Domain.CatalogManagement.Enums;
using Domain.Common.Interfaces;

namespace Domain.CatalogManagement;

public class BookContributor : IAuditableModel
{
    public required Book Book { get; set; }
    public required Contributor Contributor { get; set; }
    public required ContributorRoleEnum Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
