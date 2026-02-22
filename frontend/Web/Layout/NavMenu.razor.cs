using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.AuthManagement;

namespace LibraryApp.Web.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject]
    private IAuthStateService AuthState { get; set; } = default!;
}
