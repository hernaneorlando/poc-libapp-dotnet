using LibraryApp.Web.Model.Auth;

namespace LibraryApp.Web.Services.Auth;

/// <summary>
/// Service for managing the global authentication state.
/// Notifies subscribers when authentication state changes.
/// </summary>
public interface IAuthStateService
{
    UserLoginInfoDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    
    event Action? OnAuthStateChanged;
    
    void SetUser(UserLoginInfoDto user);
    void ClearUser();
    
    // Role-based checks
    bool HasRole(string role);
    bool HasAnyRole(params string[] roles);
    bool HasAllRoles(params string[] roles);
    
    // Permission-based checks (PBAC)
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool HasAllPermissions(params string[] permissions);
    bool HasFeaturePermission(string feature, string action);
}

public sealed class AuthStateService : IAuthStateService
{
    private UserLoginInfoDto? _currentUser;

    public UserLoginInfoDto? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            OnAuthStateChanged?.Invoke();
        }
    }

    public bool IsAuthenticated => CurrentUser != null;

    public event Action? OnAuthStateChanged;

    public void SetUser(UserLoginInfoDto user)
    {
        CurrentUser = user;
    }

    public void ClearUser()
    {
        CurrentUser = null;
    }

    // Role-based checks
    public bool HasRole(string role)
    {
        return CurrentUser?.ContainsRole(role) ?? false;
    }

    public bool HasAnyRole(params string[] roles)
    {
        if (CurrentUser == null) return false;
        return roles.Any(CurrentUser.ContainsRole);
    }

    public bool HasAllRoles(params string[] roles)
    {
        if (CurrentUser == null) return false;
        return roles.All(CurrentUser.ContainsRole);
    }

    // Permission-based checks (PBAC)
    public bool HasPermission(string permission)
    {
        return CurrentUser?.ContainsPermission(permission) ?? false;
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (CurrentUser == null) return false;
        return permissions.Any(CurrentUser.ContainsPermission);
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (CurrentUser == null) return false;
        return permissions.All(CurrentUser.ContainsPermission);
    }

    public bool HasFeaturePermission(string feature, string action)
    {
        var permission = $"{feature}:{action}";
        return HasPermission(permission);
    }
}
