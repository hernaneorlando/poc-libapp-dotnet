using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.Auth;

namespace LibraryApp.Web.Components;

public partial class UserNav : IDisposable
{
    [Inject]
    private IAuthStateService AuthState { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    private bool IsDropdownOpen = false;

    protected override void OnInitialized()
    {
        AuthState.OnAuthStateChanged += StateHasChanged;
    }

    private void ToggleDropdown()
    {
        IsDropdownOpen = !IsDropdownOpen;
    }

    private async Task HandleLogout()
    {
        IsDropdownOpen = false;
        await AuthService.LogoutAsync();
    }

    private void HandleNavigation(string url)
    {
        IsDropdownOpen = false;
        NavManager.NavigateTo(url);
    }

    private string GetUserInitials()
    {
        if (AuthState.CurrentUser == null) return "U";
        
        var names = AuthState.CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (names.Length == 0) return "U";
        if (names.Length == 1) return names[0][0].ToString().ToUpper();
        
        return $"{names[0][0]}{names[^1][0]}".ToUpper();
    }

    public void Dispose()
    {
        AuthState.OnAuthStateChanged -= StateHasChanged;
    }
}

