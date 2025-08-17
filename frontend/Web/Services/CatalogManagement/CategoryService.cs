using System.Net.Http.Json;
using System.Text;
using LibraryApp.Web.Model;
using LibraryApp.Web.Model.CatalogManagement;

namespace LibraryApp.Web.Services.CatalogManagement;

public interface ICategoryService
{
    Task<PagedResponseDTO<CategoryDto>> GetCategories(PagedRequest pagedRequest, CancellationToken? token = null);
}

public class CategoryService(IHttpClientFactory httpClientFactory) : ICategoryService
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("API");

    public async Task<PagedResponseDTO<CategoryDto>> GetCategories(PagedRequest pagedRequest, CancellationToken? token = null)
    {
        var requestUri = new StringBuilder($"api/categories?pagenumber={pagedRequest.PageNumber}&pagesize={pagedRequest.PageSize}");
        if (!string.IsNullOrWhiteSpace(pagedRequest.OrderBy))
            requestUri.Append($"&orderby={pagedRequest.OrderBy}");

        if (pagedRequest.IsDescending == true)
            requestUri.Append($"&isdescending={pagedRequest.IsDescending}");

        var categories = await httpClient.GetFromJsonAsync<PagedResponseDTO<CategoryDto>>(requestUri.ToString(), token ?? CancellationToken.None);
        return categories ?? new PagedResponseDTO<CategoryDto> { Data = []};
    }
}
