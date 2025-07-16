using Domain.Common;

namespace Domain.CatalogManagement.Specifications;

public class ActiveCategoriesSpec : Specification<Category>
{
    public ActiveCategoriesSpec(int pageNumber, int pageSize)
        : base(c => c.Active)
    {
        ApplyOrderBy(c => c.Name.Value);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}