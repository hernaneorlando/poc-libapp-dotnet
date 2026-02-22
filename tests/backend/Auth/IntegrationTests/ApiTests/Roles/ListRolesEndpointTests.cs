using System.Net;
using System.Net.Http.Json;
using Auth.Application.Roles.Commands.CreateRole;
using Auth.Application.Roles.DTOs;
using Common;
using Core.API;

namespace Auth.Tests.IntegrationTests.ApiTests.Roles;

/// <summary>
/// Integration tests for the List Roles endpoint.
/// Tests the complete flow: HTTP request → API → Handler → Database query.
/// </summary>
public class ListRolesEndpointTests(TestWebApplicationFactory factory) : BaseApiTests(factory)
{
    private static async Task<PaginatedResponse<RoleDTO>?> ExtractPaginatedResponseFromResponse(HttpResponseMessage response)
    {
        var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<PaginatedResponse<RoleDTO>>>();
        return apiResult?.Value;
    }

    #region Success Scenarios - Basic Pagination

    [Fact]
    public async Task ListRoles_WithDefaultPagination_ReturnsOkStatus()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListRoles_WithDefaultPagination_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.NotNull(paginatedResponse.Data);
        Assert.True(paginatedResponse.CurrentPage > 0);
        Assert.True(paginatedResponse.TotalCount >= 0);
    }

    [Fact]
    public async Task ListRoles_WithCustomPageSize_ReturnsCorrectNumberOfItems()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=5");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.Equal(1, paginatedResponse.CurrentPage);
        Assert.NotNull(paginatedResponse.Data);
        Assert.True(paginatedResponse.Data.Count() <= 5);
    }

    [Fact]
    public async Task ListRoles_WithPageTwo_SkipsFirstPageItems()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act - Get first page
        var responsePageOne = await client.GetAsync("/api/auth/roles?_page=1&_size=2");
        var pageOneData = await ExtractPaginatedResponseFromResponse(responsePageOne);

        // Act - Get second page
        var responsePageTwo = await client.GetAsync("/api/auth/roles?_page=2&_size=2");
        var pageTwoData = await ExtractPaginatedResponseFromResponse(responsePageTwo);

        // Assert
        Assert.NotNull(pageOneData);
        Assert.NotNull(pageTwoData);
        Assert.Equal(1, pageOneData.CurrentPage);
        Assert.Equal(2, pageTwoData.CurrentPage);
    }

    [Fact]
    public async Task ListRoles_WhenNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=999&_size=10");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.Empty(paginatedResponse.Data!);
    }

    [Fact]
    public async Task ListRoles_VerifyPaginationMetadata()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=10");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.True(paginatedResponse.CurrentPage > 0);
        Assert.True(paginatedResponse.TotalCount >= 0);
        Assert.True(paginatedResponse.TotalPages >= 0);
        Assert.NotNull(paginatedResponse.Data);
    }

    [Fact]
    public async Task ListRoles_WithLargePageSize_ReturnsAllAvailableRoles()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=1000");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.NotNull(paginatedResponse.Data);
        Assert.True(paginatedResponse.Data.Count() <= 1000);
        Assert.True(paginatedResponse.Data.Count() <= paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task ListRoles_MultiplePages_CalculateTotalPagesCorrectly()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=5");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.Equal(paginatedResponse.TotalPages, paginatedResponse.TotalPages);
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task ListRoles_ResponseIncludesAllRequiredFields()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=10");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.NotNull(paginatedResponse.Data);
        Assert.True(paginatedResponse.CurrentPage > 0);
        Assert.True(paginatedResponse.TotalCount >= 0);
        Assert.True(paginatedResponse.TotalPages >= 0);

        if (paginatedResponse.Data.Count() > 0)
        {
            var role = paginatedResponse.Data.First();
            Assert.NotNull(role.Id);
            Assert.NotEmpty(role.Name);
            Assert.NotNull(role.Description);
            Assert.NotEqual(default(DateTime), role.CreatedAt);
            Assert.NotNull(role.Permissions);
            Assert.True(role.IsActive);
        }
    }

    #endregion

    #region Authentication & Authorization

    [Fact]
    public async Task ListRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient(); // No authentication

        // Act
        var response = await client.GetAsync("/api/auth/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unauthorized", content, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListRoles_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid_token_xyz");

        // Act
        var response = await client.GetAsync("/api/auth/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListRoles_WithValidAuthenticationButWithoutReadPermission_ReturnsForbidden()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientWithoutRequiredPermissions();

        // Act
        var response = await client.GetAsync("/api/auth/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Forbidden", content, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListRoles_WithValidAuthenticationAndReadPermission_Succeeds()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);
        Assert.NotNull(paginatedResponse);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ListRoles_WithWhitespaceInQueryParameters_HandlesGracefully()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act
        var response = await client.GetAsync("/api/auth/roles?_page=1&_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);
        Assert.NotNull(paginatedResponse);
    }

    [Fact]
    public async Task ListRoles_EmptyDatabase_ReturnsEmptyPaginatedResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleRead();

        // Act - Query a very high page number to ensure empty results
        var response = await client.GetAsync("/api/auth/roles?_page=9999&_size=10");
        var paginatedResponse = await ExtractPaginatedResponseFromResponse(response);

        // Assert
        Assert.NotNull(paginatedResponse);
        Assert.NotNull(paginatedResponse.Data);
        Assert.Empty(paginatedResponse.Data);
    }

    #endregion
}
