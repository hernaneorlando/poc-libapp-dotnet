using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.Auth;

namespace LibraryApp.Web.Components;

public partial class AuthorizeView : ComponentBase, IDisposable
{
    [Inject] public IAuthStateService AuthState { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [Parameter] public RenderFragment? Authorized { get; set; }
    [Parameter] public RenderFragment? NotAuthorized { get; set; }
    [Parameter] public string? Roles { get; set; }
    [Parameter] public string? RedirectTo { get; set; } = "/login";
    [Parameter] public bool RedirectIfUnauthorized { get; set; } = true;

    protected override void OnInitialized()
    {
        AuthState.OnAuthStateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        AuthState.OnAuthStateChanged -= StateHasChanged;
    }

    private bool IsAuthorized()
    {
        if (!AuthState.IsAuthenticated)
            return false;
        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var roles = Roles.Split(',').Select(r => r.Trim()).ToArray();
            return AuthState.HasAnyRole(roles);
        }
        return true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !IsAuthorized() && RedirectIfUnauthorized)
        {
            var returnUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
            Navigation.NavigateTo($"{RedirectTo}?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
        }
    }
}
