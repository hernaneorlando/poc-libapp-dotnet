using Domain.CatalogManagement.Enums;
using Domain.Common;

namespace Domain.CatalogManagement;

public class BookContributor : RelationalDbAuditableModel
{
    public required Book Book { get; set; }
    public required Contributor Contributor { get; set; }
    public required ContributorRoleEnum Role { get; set; }
}
