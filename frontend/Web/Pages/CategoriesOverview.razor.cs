using LibraryApp.Web.Model;
using LibraryApp.Web.Model.CatalogManagement;
using LibraryApp.Web.Services.CatalogManagement;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Pages;

public partial class CategoriesOverview
{
    [Inject]
    public ICategoryService CategoryService { get; set; } = null!;

    private List<CategoryDto> categories = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var pagedRequest = new PagedRequest(1, 100);
            var response = await CategoryService.GetCategories(pagedRequest, CancellationToken.None);
            categories = response.Data?.ToList() ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar categorias: {ex.Message}");
        }
    }
}
