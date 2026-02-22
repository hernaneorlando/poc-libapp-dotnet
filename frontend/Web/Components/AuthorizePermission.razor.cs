using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.AuthManagement;

namespace LibraryApp.Web.Components;

/// <summary>
/// Component for conditional rendering based on user permissions (PBAC).
/// Checks if the current user has the required permissions.
/// </summary>
public partial class AuthorizePermission : ComponentBase, IDisposable
{
    [Inject]
    private IAuthStateService AuthState { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// Content to render when the user is authorized.
    /// </summary>
    [Parameter]
    public RenderFragment? Authorized { get; set; }

    /// <summary>
    /// Content to render when the user is not authorized.
    /// </summary>
    [Parameter]
    public RenderFragment? NotAuthorized { get; set; }

    /// <summary>
    /// Comma-separated list of permissions (e.g., "Book:Create,Book:Update").
    /// User must have at least one of these permissions.
    /// </summary>
    [Parameter]
    public string? Permissions { get; set; }

    /// <summary>
    /// When true, user must have ALL specified permissions.
    /// When false (default), user needs at least ONE permission.
    /// </summary>
    [Parameter]
    public bool RequireAll { get; set; } = false;

    /// <summary>
    /// Feature name (e.g., "Book", "Category", "User").
    /// Used with Action parameter to construct permission code.
    /// </summary>
    [Parameter]
    public string? Feature { get; set; }

    /// <summary>
    /// Action name (e.g., "Create", "Read", "Update", "Delete").
    /// Used with Feature parameter to construct permission code.
    /// </summary>
    [Parameter]
    public string? Action { get; set; }

    /// <summary>
    /// URL to redirect to if user is not authorized.
    /// Default is "/login".
    /// </summary>
    [Parameter]
    public string RedirectTo { get; set; } = "/login";

    /// <summary>
    /// If true (default), automatically redirects unauthorized users.
    /// If false, only renders NotAuthorized content.
    /// </summary>
    [Parameter]
    public bool RedirectIfUnauthorized { get; set; } = false;

    protected override void OnInitialized()
    {
        // Subscribe to auth state changes
        AuthState.OnAuthStateChanged += HandleAuthStateChanged;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && RedirectIfUnauthorized && !IsAuthorized())
        {
            var returnUrl = Uri.EscapeDataString(Navigation.Uri.Replace(Navigation.BaseUri, "/"));
            Navigation.NavigateTo($"{RedirectTo}?returnUrl={returnUrl}");
        }
    }

    private void HandleAuthStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public bool IsAuthorized()
    {
        if (!AuthState.IsAuthenticated)
            return false;

        // If Feature and Action are specified, check feature permission
        if (!string.IsNullOrWhiteSpace(Feature) && !string.IsNullOrWhiteSpace(Action))
        {
            return AuthState.HasFeaturePermission(Feature, Action);
        }

        // If Permissions parameter is specified, check permissions
        if (!string.IsNullOrWhiteSpace(Permissions))
        {
            var permissions = Permissions.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            if (permissions.Length == 0)
                return false;

            return RequireAll
                ? AuthState.HasAllPermissions(permissions)
                : AuthState.HasAnyPermission(permissions);
        }

        // No permissions specified - allow if authenticated
        return true;
    }

    public void Dispose()
    {
        AuthState.OnAuthStateChanged -= HandleAuthStateChanged;
    }
}
