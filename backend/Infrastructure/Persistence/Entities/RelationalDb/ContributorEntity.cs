using Domain.CatalogManagement;
using Infrastructure.Common;
using Infrastructure.Persistence.SeedWork;

namespace Infrastructure.Persistence.Entities.RelationalDb;

public class ContributorEntity : RelationalDbBaseBaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public IList<BookContributorEntity> Books { get; set; } = [];

    public static implicit operator Contributor(ContributorEntity entity)
    {
        var model = new Contributor()
        {
            Id = entity.ExternalId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth,
            Books = [.. entity.Books.Select(book => (BookContributor)book)]
        };

        model.ConvertEntityBaseProperties(entity);
        return model;
    }

    public static implicit operator ContributorEntity(Contributor contributor)
    {
        var entity = new ContributorEntity()
        {
            ExternalId = contributor.Id,
            FirstName = contributor.FirstName,
            LastName = contributor.LastName,
            DateOfBirth = contributor.DateOfBirth,
            Books = [.. contributor.Books.Select(book => (BookContributorEntity)book)]
        };

        entity.ConvertModelBaseProperties(contributor);
        return entity;
    }
}