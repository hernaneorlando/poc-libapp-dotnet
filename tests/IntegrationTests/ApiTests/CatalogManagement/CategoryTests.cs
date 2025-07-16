using System.Net.Http.Json;
using Application.CatalogManagement.Books.DTOs;
using IntegrationTests.ApiTests.Common;
using LibraryApp.API;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.ApiTests.CatalogManagement;

public class CategoryTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task PostCategory_CreatesCategorySuccessfully()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy",
            Description = "Some fantasy book."
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.True(category.Active);
        Assert.True(category.Id != default);
        Assert.Equal("Fantasy", category.Name);
        Assert.Equal("Some fantasy book.", category.Description);
        Assert.Empty(category.Books);
        Assert.True(category.CreatedAt != default);
        Assert.Null(category.UpdatedAt);
    }
}