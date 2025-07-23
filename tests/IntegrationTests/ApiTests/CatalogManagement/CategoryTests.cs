using System.Net;
using System.Net.Http.Json;
using Application.CatalogManagement.Books.DTOs;
using Domain.CatalogManagement;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.RelationalDb;
using IntegrationTests.ApiTests.Common;
using LibraryApp.API;
using LibraryApp.API.Extension;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.ApiTests.CatalogManagement;

public class CategoryCreateTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task PostCategory_CreatesCategory_ReturnSuccess()
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

    [Fact]
    public async Task PostCategory_CreatesCategoryWithoutDescription_ReturnSuccess()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy"
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
        Assert.Empty(category.Description!);
        Assert.Empty(category.Books);
        Assert.True(category.CreatedAt != default);
        Assert.Null(category.UpdatedAt);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task PostCategory_CreatesCategoryWithEmptyInputs_ReturnError(string? content)
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = content,
            Description = content
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category creation failed", result.Title);
        Assert.Equal("At least one field must be set to be created", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task PostCategory_CreatesCategoryWithInvalidInputs_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = new string('n', 51),
            Description = new string('d', 201)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category creation failed", result.Title);
        Assert.Equal("Category Name must not exceed 50 characters,\r\nCategory Description must not exceed 200 characters", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }
}

public class CategoryReadTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task GetCategory_ReturnsCategoryDetails()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var newCategory = new Category("Science Fiction")
        {
            Description = "Books about sci-fi."
        };

        dbContext.Set<CategoryEntity>().Add(newCategory);
        await dbContext.SaveChangesAsync();

        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/categories/{newCategory.ExternalId}");

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.Equal(newCategory.ExternalId, category.Id);
        Assert.Equal("Science Fiction", category.Name);
        Assert.Equal("Books about sci-fi.", category.Description);
        Assert.True(category.Active);
        Assert.Empty(category.Books);
        Assert.True(category.CreatedAt != default);
        Assert.Null(category.UpdatedAt);
    }

    [Fact]
    public async Task GetCategory_NonExistentCategory_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var nonExistentCategoryId = Guid.NewGuid();

        // Act
        var getResponse = await client.GetAsync($"/api/categories/{nonExistentCategoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var categoryResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(categoryResult);
        Assert.Equal("Category not found", categoryResult.Title);
        Assert.Equal($"Category with ID {nonExistentCategoryId} not found.", categoryResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, categoryResult.StatusCode);
    }

    [Fact]
    public async Task GetCategory_InvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var nonExistentCategoryId = "123456789";

        // Act
        var getResponse = await client.GetAsync($"/api/categories/{nonExistentCategoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var categoryResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(categoryResult);
        Assert.Equal("Category not found", categoryResult.Title);
        Assert.Equal("External Id must be a valid GUID", categoryResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, categoryResult.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ReturnsCategoriesList()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var newCategoryList = new[] {
            new Category("Inactive"){
                Description = "Some inactive category",
                Active = false,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new Category("Science Fiction")
            {
                Description = "Books about sci-fi.",
                Active = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new Category("Fantasy"){
                Description = "Some fantasy book.",
                Active = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        await dbContext.Set<CategoryEntity>().AddRangeAsync(newCategoryList.Select(c => (CategoryEntity)c));
        await dbContext.SaveChangesAsync();

        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/categories?pagenumber=1&pagesize=10");

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var categories = (await getResponse.Content.ReadFromJsonAsync<CategoryDto[]>())!;
        Assert.NotEmpty(categories);
        Assert.All(categories, x => Assert.True(x.Active));
        Assert.DoesNotContain(newCategoryList[0], categories);

        var expectedResult = newCategoryList
            .Where(c => c.Active)
            .Select(category => (CategoryDto)category)
            .OrderBy(c => c.Name)
            .ToArray();

        Assert.Equal(expectedResult.Length, categories.Length);
        for (int i = 0; i < expectedResult.Length; i++)
        {
            Assert.Equal(expectedResult[i].Id, categories[i].Id);
            Assert.Equal(expectedResult[i].Name, categories[i].Name);
            Assert.Equal(expectedResult[i].Description, categories[i].Description);
        }
    }

    [Fact]
    public async Task GetCategories_NonExistentCategoreis_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/categories?pagenumber=1&pagesize=10");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var categoryResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(categoryResult);
        Assert.Equal("Categories not found", categoryResult.Title);
        Assert.Equal("No active categories found.", categoryResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, categoryResult.StatusCode);
    }

    [Fact]
    public async Task GetCategories_NoInputs_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        var categoryResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(categoryResult);
        Assert.Equal("Validation Error", categoryResult.Title);

        var errorMessage = @"Validation failed: 
 -- PageNumber: Page number must be greater than 0. Severity: Error
 -- PageSize: Page size must be between 1 and 100. Severity: Error";

        Assert.Equal(errorMessage, categoryResult.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, categoryResult.StatusCode);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task GetCategories_InvalidInputs_ReturnsFailure(int pageNumber, int pageSize)
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/categories?pagenumber={pageNumber}&pagesize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        var categoryResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(categoryResult);
        Assert.Equal("Validation Error", categoryResult.Title);

        var errorMessage = @"Validation failed: 
 -- PageNumber: Page number must be greater than 0. Severity: Error
 -- PageSize: Page size must be between 1 and 100. Severity: Error";

        Assert.Equal(errorMessage, categoryResult.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, categoryResult.StatusCode);
    }
}

public class CategoryUpdateTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task PutCategory_UpdateCategory_ReturnSuccess()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var newCategory = new Category("Science Fiction")
        {
            Description = "Books about sci-fi."
        };

        var newEntity = (CategoryEntity)newCategory;
        dbContext.Set<CategoryEntity>().Add(newEntity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(newEntity).State = EntityState.Detached;

        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy",
            Description = "Some fantasy book."
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{newCategory.ExternalId}", request);

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
        Assert.True(category.UpdatedAt != default);
    }

    [Fact]
    public async Task PutCategory_UpdateNonExistentCategory_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy",
            Description = "Some fantasy book."
        };

        var categoryId = Guid.NewGuid();

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{categoryId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category update failed", result.Title);
        Assert.Equal($"Category with ID {categoryId} not found.", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task PutCategory_UpdateCategoryWithoutDescription_ReturnSuccess()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var categoryName = "Science Fiction";
        var newCategory = new Category(categoryName)
        {
            Description = "Books about sci-fi."
        };

        var newEntity = (CategoryEntity)newCategory;
        dbContext.Set<CategoryEntity>().Add(newEntity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(newEntity).State = EntityState.Detached;

        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = categoryName
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{newCategory.ExternalId}", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.True(category.Active);
        Assert.True(category.Id != default);
        Assert.Equal(categoryName, category.Name);
        Assert.Empty(category.Description!);
        Assert.Empty(category.Books);
        Assert.True(category.CreatedAt != default);
        Assert.True(category.UpdatedAt != default);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task PutCategory_UpdateCategoryWithEmptyInputs_ReturnError(string? content)
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var categoryName = "Science Fiction";
        var newCategory = new Category(categoryName)
        {
            Description = "Books about sci-fi."
        };

        var newEntity = (CategoryEntity)newCategory;
        dbContext.Set<CategoryEntity>().Add(newEntity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(newEntity).State = EntityState.Detached;

        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = content,
            Description = content
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{newCategory.ExternalId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category update failed", result.Title);
        Assert.Equal("At least one field must be set to be updated", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task PutCategory_UpdateCategoryWithInvalidInputs_ReturnError()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var categoryName = "Science Fiction";
        var newCategory = new Category(categoryName)
        {
            Description = "Books about sci-fi."
        };

        var newEntity = (CategoryEntity)newCategory;
        dbContext.Set<CategoryEntity>().Add(newEntity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(newEntity).State = EntityState.Detached;

        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = new string('n', 51),
            Description = new string('d', 201)
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{newCategory.ExternalId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category update failed", result.Title);
        Assert.Equal("Category Name must not exceed 50 characters,\r\nCategory Description must not exceed 200 characters", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task PutCategory_InvalidId_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy",
            Description = "Some fantasy book."
        };

        var categoryId = "123456789";

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{categoryId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category update failed", result.Title);
        Assert.Equal("External Id must be a valid GUID", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }
}

public class CategoryDeleteTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task DeleteCategory_ArchiveCategory_ReturnSuccess()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<SqlDataContext>()!;

        var newCategory = new Category("Science Fiction")
        {
            Description = "Books about sci-fi."
        };

        var newEntity = (CategoryEntity)newCategory;
        dbContext.Set<CategoryEntity>().Add(newEntity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(newEntity).State = EntityState.Detached;

        var client = _webFactory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/categories/{newCategory.ExternalId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DeleteCategory_ArchiveNonExistentCategory_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var categoryId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category deletion failed", result.Title);
        Assert.Equal($"Category with ID {categoryId} not found.", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_InvalidId_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var categoryId = "123456789";

        // Act
        var response = await client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Category deletion failed", result.Title);
        Assert.Equal("External Id must be a valid GUID", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }
}