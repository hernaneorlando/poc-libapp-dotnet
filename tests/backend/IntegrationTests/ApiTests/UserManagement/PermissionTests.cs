using System.Net;
using System.Net.Http.Json;
using Application.Common.BaseDTO;
using Application.UserManagement.Permissions.DTOs;
using Domain.UserManagement;
using Domain.UserManagement.Enums;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DocumentDb;
using IntegrationTests.ApiTests.Common;
using LibraryApp.API;
using LibraryApp.API.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.ApiTests.UserManagement;

public class PermissionReadTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task GetPermission_ReturnsPermissionDetails()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<NoSqlDataContext>()!;

        var newEntity = (PermissionEntity)Permission.Create(
            PermissionFeature.Book,
            PermissionAction.Create,
            "Books creation authorization.");

        await dbContext.Permissions.InsertOneAsync(newEntity);

        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions/{newEntity.ExternalId}");

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var permission = await getResponse.Content.ReadFromJsonAsync<PermissionDto>();
        Assert.NotNull(permission);
        Assert.Equal(newEntity.ExternalId, permission.Id);
        Assert.Equal("Books creation authorization.", permission.Description);
        Assert.True(permission.Active);
        Assert.True(permission.CreatedAt != default);
        Assert.Null(permission.UpdatedAt);
    }

    [Fact]
    public async Task GetPermission_NonExistentPermission_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var nonExistentPermissionId = Guid.NewGuid();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions/{nonExistentPermissionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var permissionResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(permissionResult);
        Assert.Equal("Permission not found", permissionResult.Title);
        Assert.Equal($"Permission with ID {nonExistentPermissionId} not found.", permissionResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, permissionResult.StatusCode);
    }

    [Fact]
    public async Task GetPermission_InvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var nonExistentPermissionId = "123456789";

        // Act
        var getResponse = await client.GetAsync($"/api/permissions/{nonExistentPermissionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var permissionResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(permissionResult);
        Assert.Equal("Permission not found", permissionResult.Title);
        Assert.Equal("External Id must be a valid GUID", permissionResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, permissionResult.StatusCode);
    }

    [Fact]
    public async Task GetPermissions_ReturnsPermissionsList()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<NoSqlDataContext>()!;

        var newPermissionList = new[] {
            new PermissionEntity
            {
                Code = $"{PermissionFeature.Book}:{PermissionAction.Create}",
                Description = "Some inactive Permission",
                Active = false,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new PermissionEntity
            {
                Code = $"{PermissionFeature.Book}:{PermissionAction.Update}",
                Description = "Book creation authorization",
                Active = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new PermissionEntity
            {
                Code = $"{PermissionFeature.Book}:{PermissionAction.Read}",
                Description = "Book reading authorization",
                Active = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        await dbContext.Permissions.InsertManyAsync(newPermissionList);

        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions?pagenumber=1&pagesize=10");

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var response = (await getResponse.Content.ReadFromJsonAsync<PagedResponseDTO<PermissionDto>>())!;
        Assert.NotNull(response?.Data);

        var permissions = response.Data.ToArray();
        Assert.NotEmpty(permissions);
        Assert.All(permissions, x => Assert.True(x.Active));
        Assert.DoesNotContain(newPermissionList[0], permissions);

        var expectedResult = newPermissionList
            .Where(p => p.Active)
            .Select(p => (PermissionDto)p)
            .OrderBy(p => p.Code)
            .ToArray();

        Assert.Equal(expectedResult.Length, permissions.Length);
        for (int i = 0; i < expectedResult.Length; i++)
        {
            Assert.Equal(expectedResult[i].Id, permissions[i].Id);
            Assert.Equal(expectedResult[i].Code, permissions[i].Code);
            Assert.Equal(expectedResult[i].Description, permissions[i].Description);
        }
    }

    [Fact]
    public async Task GetPermissions_NonExistentCategoreis_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions?pagenumber=1&pagesize=10");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var permissionResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(permissionResult);
        Assert.Equal("Permissions not found", permissionResult.Title);
        Assert.Equal("No active permissions found.", permissionResult.Details);
        Assert.Equal((int)HttpStatusCode.NotFound, permissionResult.StatusCode);
    }

    [Fact]
    public async Task GetPermissions_NoInputs_ReturnsFailure()
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        var permissionResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(permissionResult);
        Assert.Equal("Validation Error", permissionResult.Title);

        var errorMessage = @"Validation failed: 
 -- PageNumber: Page number must be greater than 0. Severity: Error
 -- PageSize: Page size must be between 1 and 100. Severity: Error";

        Assert.Equal(errorMessage, permissionResult.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, permissionResult.StatusCode);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task GetPermissions_InvalidInputs_ReturnsFailure(int pageNumber, int pageSize)
    {
        // Arrange
        var client = _webFactory.CreateClient();

        // Act
        var getResponse = await client.GetAsync($"/api/permissions?pagenumber={pageNumber}&pagesize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        var permissionResult = await getResponse.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(permissionResult);
        Assert.Equal("Validation Error", permissionResult.Title);

        var errorMessage = @"Validation failed: 
 -- PageNumber: Page number must be greater than 0. Severity: Error
 -- PageSize: Page size must be between 1 and 100. Severity: Error";

        Assert.Equal(errorMessage, permissionResult.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, permissionResult.StatusCode);
    }
}

public class PermissionUpdateTests(WebApplicationFactory<Program> factory) : BaseApiTests(factory)
{
    [Fact]
    public async Task PutPermission_UpdatePermission_ReturnSuccess()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<NoSqlDataContext>()!;

        var newEntity = (PermissionEntity)Permission.Create(
            PermissionFeature.Book,
            PermissionAction.Create,
            "Books creation authorization.");

        await dbContext.Permissions.InsertOneAsync(newEntity);

        var client = _webFactory.CreateClient();
        var request = new
        {
            Description = "Description changed"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/permissions/{newEntity.ExternalId}", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var permission = await response.Content.ReadFromJsonAsync<PermissionDto>();
        Assert.NotNull(permission);
        Assert.True(permission.Active);
        Assert.True(permission.Id != default);
        Assert.Equal(request.Description, permission.Description);
        Assert.True(permission.CreatedAt != default);
        Assert.True(permission.UpdatedAt != default);
    }

    [Fact]
    public async Task PutPermission_UpdateNonExistentPermission_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Description = "Some description."
        };

        var permissionId = Guid.NewGuid();

        // Act
        var response = await client.PutAsJsonAsync($"/api/permissions/{permissionId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Permission update failed", result.Title);
        Assert.Equal($"Permission with ID {permissionId} not found.", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task PutPermission_UpdatePermissionWithoutDescription_ReturnSuccess(string? content)
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<NoSqlDataContext>()!;

        var newEntity = (PermissionEntity)Permission.Create(
            PermissionFeature.Book,
            PermissionAction.Create,
            "Books creation authorization.");

        await dbContext.Permissions.InsertOneAsync(newEntity);

        var client = _webFactory.CreateClient();
        var request = new
        {
            Description = content
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Permissions/{newEntity.ExternalId}", request);

         // Assert
        response.EnsureSuccessStatusCode();
        var permission = await response.Content.ReadFromJsonAsync<PermissionDto>();
        Assert.NotNull(permission);
        Assert.True(permission.Active);
        Assert.True(permission.Id != default);
        Assert.True(string.IsNullOrWhiteSpace(permission.Description));
        Assert.True(permission.CreatedAt != default);
        Assert.True(permission.UpdatedAt != default);
    }

    [Fact]
    public async Task PutPermission_UpdatePermissionWithInvalidInputs_ReturnError()
    {
        // Arrange
        var dbContext = _webFactory.Services
            .CreateScope()
            .ServiceProvider.GetService<NoSqlDataContext>()!;

        var newEntity = (PermissionEntity)Permission.Create(
            PermissionFeature.Book,
            PermissionAction.Create,
            "Books creation authorization.");

        await dbContext.Permissions.InsertOneAsync(newEntity);

        var client = _webFactory.CreateClient();
        var request = new
        {
            Description = new string('d', 257)
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Permissions/{newEntity.ExternalId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Permission update failed", result.Title);
        Assert.Equal("Permission Description must not exceed 256 characters", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task PutPermission_InvalidId_ReturnError()
    {
        // Arrange
        var client = _webFactory.CreateClient();
        var request = new
        {
            Name = "Fantasy",
            Description = "Some fantasy book."
        };

        var PermissionId = "123456789";

        // Act
        var response = await client.PutAsJsonAsync($"/api/Permissions/{PermissionId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        Assert.NotNull(result);
        Assert.Equal("Permission update failed", result.Title);
        Assert.Equal("External Id must be a valid GUID", result.Details);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }
}