using Domain.CatalogManagement;
using Domain.CatalogManagement.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class BookContributorEntity : RelationalDbAuditableEntity
{
    public BookEntity Book { get; set; } = new();
    public long BookId { get; set; }
    public ContributorEntity Contributor { get; set; } = new();
    public long ContributorId { get; set; }
    public ContributorRoleEnum Role { get; set; } = ContributorRoleEnum.MainAuthor;

    public static implicit operator BookContributor(BookContributorEntity bookContributorEntity)
    {
        return new BookContributor
        {
            Book = bookContributorEntity.Book,
            Contributor = bookContributorEntity.Contributor,
            Role = bookContributorEntity.Role,
            CreatedAt = bookContributorEntity.CreatedAt,
            UpdatedAt = bookContributorEntity.UpdatedAt,
        };
    }

    public static implicit operator BookContributorEntity(BookContributor bookContributor)
    {
        return new BookContributorEntity
        {
            Book = bookContributor.Book,
            Contributor = bookContributor.Contributor,
            Role = bookContributor.Role,
            CreatedAt = bookContributor.CreatedAt,
            UpdatedAt = bookContributor.UpdatedAt,
        };
    }
}
