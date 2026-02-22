using LibraryApp.Web.Model;
using LibraryApp.Web.Model.UserManagement;
using LibraryApp.Web.Services.UserManagement;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Pages.UserManagement;

public partial class RolesOverview
{
    [Inject]
    public IRoleService RoleService { get; set; } = null!;

    private List<RoleDto> roles = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var pagedRequest = new PagedRequest(1, 100);
            var response = await RoleService.GetRoles(pagedRequest, CancellationToken.None);
            roles = response.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar roles: {ex.Message}");
        }
    }

    private void EditRole(Guid id)
    {
        Console.WriteLine($"Edit role: {id}");
        // TODO: Implement edit functionality
    }

    private void DeleteRole(Guid id)
    {
        Console.WriteLine($"Delete role: {id}");
        // TODO: Implement delete functionality
    }
}
