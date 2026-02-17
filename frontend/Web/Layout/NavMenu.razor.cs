using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.Auth;

namespace LibraryApp.Web.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject]
    private IAuthStateService AuthState { get; set; } = default!;
}
