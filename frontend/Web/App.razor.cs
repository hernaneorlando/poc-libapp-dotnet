using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.Auth;

namespace LibraryApp.Web;

public partial class App
{
    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        // Attempt to restore session from stored tokens
        await AuthService.RestoreSessionAsync();
    }
}
