using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LibraryApp.Web.Model;
using LibraryApp.Web.Model.AuthManagement;
using LibraryApp.Web.Model.UserManagement;

namespace LibraryApp.Web.Services.UserManagement;

public interface IRoleService
{
    Task<PagedResponseDTO<RoleDto>> GetRoles(PagedRequest pagedRequest, CancellationToken? token = null);
}

public class RoleService : IRoleService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RoleService(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient("API");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PagedResponseDTO<RoleDto>> GetRoles(PagedRequest pagedRequest, CancellationToken? token = null)
    {
        var requestUri = new StringBuilder($"api/auth/roles?_page={pagedRequest.PageNumber}&_size={pagedRequest.PageSize}");
        
        if (!string.IsNullOrWhiteSpace(pagedRequest.OrderBy))
            requestUri.Append($"&_order={pagedRequest.OrderBy}");

        // if (pagedRequest.IsDescending == true)
        //     requestUri.Append($"&_desc={pagedRequest.IsDescending}");

        var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri.ToString()), token ?? CancellationToken.None);
        if (!result.IsSuccessStatusCode)
        {
            return new PagedResponseDTO<RoleDto> { Data = []};
        } 

        var roles = await JsonSerializer.DeserializeAsync<ApiResultDto<PagedResponseDTO<RoleDto>>>(await result.Content.ReadAsStreamAsync(), _jsonOptions, token ?? CancellationToken.None);
        if (roles == null || !roles.IsSuccess || roles.Value == null)
        {
            return new PagedResponseDTO<RoleDto> { Data = []};
        }

        return roles?.Value ?? new PagedResponseDTO<RoleDto> { Data = []};
    }
}
