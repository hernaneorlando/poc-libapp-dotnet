using System.Linq.Expressions;
using Core.Infrastructure;

namespace Auth.Infrastructure.Specifications;

public class GetUserByUsername(string username) : ISpecification<UserEntity>
{
    public Expression<Func<UserEntity, bool>> Predicate {get;private set;} = 
        user => user.Username == username && user.IsActive;

    public IEnumerable<SpecificationInclude<UserEntity>> Includes =>
    [
        new(user => user.UserRoles) 
        {
            ThenIncludeExpression = userRole => ((UserRoleEntity)userRole).Role
        }
    ];
}
