using System.Linq.Expressions;
using Core.Infrastructure;

namespace Auth.Infrastructure.Specifications;

public class GetUserByEmail(string email) : ISpecification<UserEntity>
{
    public Expression<Func<UserEntity, bool>> Predicate {get;private set;} = 
        user => user.Email == email && user.IsActive;

    public IEnumerable<SpecificationInclude<UserEntity>> Includes => [];
}
