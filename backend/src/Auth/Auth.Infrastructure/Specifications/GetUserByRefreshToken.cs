using System.Linq.Expressions;
using Core.Infrastructure;

namespace Auth.Infrastructure.Specifications;

public class GetUserByRefreshToken(string refreshToken) : ISpecification<UserEntity>
{
    public Expression<Func<UserEntity, bool>> Predicate {get; private set;} = 
        user => user.RefreshTokens.Any(rt => rt.Token == refreshToken) && user.IsActive;

    public IEnumerable<SpecificationInclude<UserEntity>> Includes =>
    [
        new (user => user.RefreshTokens),
        new (user => user.UserRoles)
        {
            ThenIncludeExpression = userRole => ((UserRoleEntity)userRole).Role
        }
    ];
}
