using LibraryApp.Web.Model;
using LibraryApp.Web.Model.CatalogManagement;
using LibraryApp.Web.Services.CatalogManagement;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Pages.CatalogManagement;

public partial class CategoriesOverview
{
    [Inject]
    public ICategoryService CategoryService { get; set; } = null!;

    private List<CategoryDto> categories = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var pagedRequest = new PagedRequest(1, 100);
            var response = await CategoryService.GetCategories(pagedRequest, CancellationToken.None);
            categories = response.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar categorias: {ex.Message}");
        }
    }

    private void EditCategory(Guid id)
    {
        Console.WriteLine($"Edit category: {id}");
        // TODO: Implement edit functionality
    }

    private void DeleteCategory(Guid id)
    {
        Console.WriteLine($"Delete category: {id}");
        // TODO: Implement delete functionality
    }
}
