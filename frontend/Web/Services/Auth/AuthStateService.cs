using LibraryApp.Web.Model.Auth;
using LibraryApp.Web.Model.Auth.Enums;

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

    // Permission-based checks (PBAC)
    public bool HasPermission(string permission)
    {
        if (CurrentUser == null)
            return false;

        if (CurrentUser.UserType == UserType.Administrator)
            return true; // Admins have all permissions

        return CurrentUser.ContainsPermission(permission);
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (CurrentUser == null)
            return false;

        if (CurrentUser.UserType == UserType.Administrator)
            return true; // Admins have all permissions

        return permissions.Any(CurrentUser.ContainsPermission);
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (CurrentUser == null)
            return false;

        if (CurrentUser.UserType == UserType.Administrator)
            return true; // Admins have all permissions

        return permissions.All(CurrentUser.ContainsPermission);
    }

    public bool HasFeaturePermission(string feature, string action)
    {
        var permission = $"{feature}:{action}";
        return HasPermission(permission);
    }
}
