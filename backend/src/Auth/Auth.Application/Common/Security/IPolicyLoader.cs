namespace Auth.Application.Common.Security;

public interface IPolicyLoader
{
    Task<IEnumerable<AuthPolicy>> LoadPoliciesAsync();
    Task<bool> NeedsPolicyUpdateAsync();
}
