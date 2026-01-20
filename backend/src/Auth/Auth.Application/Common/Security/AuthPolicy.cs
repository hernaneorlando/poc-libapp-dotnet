using Microsoft.AspNetCore.Authorization;

namespace Auth.Application.Common.Security;

public class AuthPolicy
{
   public string Name { get; set; }
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> RequiredRoles { get; set; } = new();

    public AuthorizationPolicy Build()
    {
        var builder = new AuthorizationPolicyBuilder();
        
        if (RequiredPermissions.Count != 0)
            builder.RequireClaim("Permission", RequiredPermissions);
            
        if (RequiredRoles.Count != 0)
            builder.RequireRole(RequiredRoles);
            
        return builder.Build();
    }
}