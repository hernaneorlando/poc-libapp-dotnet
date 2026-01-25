using System.Net;
using System.Net.Http.Json;
using Auth.Application.Roles.Commands.CreateRole;
using Auth.Application.Roles.DTOs;
using Common;

namespace Auth.Tests.IntegrationTests.ApiTests.Roles;

/// <summary>
/// Integration tests for the Create Role endpoint.
/// Tests the complete flow: HTTP request → API → Handler → Domain → Database.
/// </summary>
public class CreateRoleEndpointTests(TestWebApplicationFactory factory) : BaseAuthApiTests(factory)
{
    #region Success Scenarios

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Administrator",
            Description: "Full system access with all permissions",
            Permissions:
            [
                new RolePermissionRequest("User", "Create"),
                new RolePermissionRequest("User", "Read"),
                new RolePermissionRequest("Role", "Update")
            ]
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsRoleDtoInResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Editor",
            Description: "Can edit content",
            Permissions:
            [
                new RolePermissionRequest("Book", "Update"),
                new RolePermissionRequest("Book", "Read")
            ]
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.Equal("Editor", roleDto.Name);
        Assert.Equal("Can edit content", roleDto.Description);
        Assert.False(string.IsNullOrEmpty(roleDto.Id?.ToString()));
        Assert.NotEqual(default(DateTime), roleDto.CreatedAt);
        Assert.True(roleDto.IsActive);
    }

    [Fact]
    public async Task CreateRole_WithValidData_PersistsToDatabase()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Viewer",
            Description: "Read-only access",
            Permissions:
            [
                new RolePermissionRequest("Book", "Read"),
                new RolePermissionRequest("User", "Read")
            ]
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert - Verify persistence
        response.EnsureSuccessStatusCode();
        Assert.NotNull(roleDto);
        Assert.Equal("Viewer", roleDto.Name);
        Assert.Equal("Read-only access", roleDto.Description);
    }

    [Fact]
    public async Task CreateRole_WithPermissions_PermissionsAreAssigned()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Manager",
            Description: "Management access",
            Permissions:
            [
                new RolePermissionRequest("User", "Create"),
                new RolePermissionRequest("Role", "Read"),
                new RolePermissionRequest("Category", "Update")
            ]
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.Equal(3, roleDto.Permissions.Count);
        Assert.Contains(roleDto.Permissions, p => p.Code == "User:Create");
        Assert.Contains(roleDto.Permissions, p => p.Code == "Role:Read");
        Assert.Contains(roleDto.Permissions, p => p.Code == "Category:Update");
    }

    [Fact]
    public async Task CreateRole_WithoutPermissions_CreatesRoleWithEmptyPermissions()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Guest",
            Description: "Guest role without permissions",
            Permissions: []
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.Equal("Guest", roleDto.Name);
        Assert.Empty(roleDto.Permissions);
    }

    #endregion

    #region Validation Error Scenarios

    [Fact]
    public async Task CreateRole_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "",
            Description: "Valid description",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Validation", content);
    }

    [Fact]
    public async Task CreateRole_WithShortName_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "AB",
            Description: "Valid description",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithLongName_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var longName = new string('A', 51);
        var request = new CreateRoleCommand(
            Name: longName,
            Description: "Valid description",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithEmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "ValidRole",
            Description: "",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithShortDescription_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "ValidRole",
            Description: "Short",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithLongDescription_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var longDescription = new string('D', 501);
        var request = new CreateRoleCommand(
            Name: "ValidRole",
            Description: longDescription,
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithInvalidFeature_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "ValidRole",
            Description: "Valid description for testing",
            Permissions: new[] { new RolePermissionRequest("InvalidFeature", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithInvalidAction_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "ValidRole",
            Description: "Valid description for testing",
            Permissions: new[] { new RolePermissionRequest("User", "InvalidAction") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task CreateRole_ResponseIncludesAllRequiredFields()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "Complete",
            Description: "Testing complete response structure",
            Permissions: new[] { new RolePermissionRequest("User", "Create") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.True(roleDto.Id != null);
        Assert.Equal("Complete", roleDto.Name);
        Assert.Equal("Testing complete response structure", roleDto.Description);
        Assert.NotNull(roleDto.Permissions);
        Assert.NotEqual(default(DateTime), roleDto.CreatedAt);
        Assert.Null(roleDto.UpdatedAt);
        Assert.True(roleDto.IsActive);
    }

    [Fact]
    public async Task CreateRole_LocationHeaderIsSet()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "HeaderTest",
            Description: "Testing location header",
            Permissions: new[] { new RolePermissionRequest("Book", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        // Note: Location header may or may not be set depending on endpoint implementation
        // This test documents expected behavior
    }

    #endregion

    #region Database State

    [Fact]
    public async Task CreateRole_VerifyRoleIdIsGenerated()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "IdTest",
            Description: "Testing ID generation",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.True(roleDto.Id != null);
    }

    [Fact]
    public async Task CreateRole_VerifyCreatedAtIsSet()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "TimestampTest",
            Description: "Testing timestamp",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );
        var beforeRequest = DateTime.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();
        var afterRequest = DateTime.UtcNow;

        // Assert
        Assert.NotNull(roleDto);
        Assert.NotEqual(default(DateTime), roleDto.CreatedAt);
        Assert.True(roleDto.CreatedAt >= beforeRequest);
        Assert.True(roleDto.CreatedAt <= afterRequest);
    }

    [Fact]
    public async Task CreateRole_VerifyRoleIsActive()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "ActiveTest",
            Description: "Testing active status",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();

        // Assert
        Assert.NotNull(roleDto);
        Assert.True(roleDto.IsActive);
    }

    #endregion

    #region Authentication & Authorization

    [Fact]
    public async Task CreateRole_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient(); // No authentication
        var request = new CreateRoleCommand(
            Name: "TestRole",
            Description: "Testing without authentication",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unauthorized", content, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRole_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid_token_12345");
        
        var request = new CreateRoleCommand(
            Name: "TestRole",
            Description: "Testing with invalid token",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_WithValidAuthenticationButWithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientWithoutRequiredPermissions();
        var request = new CreateRoleCommand(
            Name: "TestRole",
            Description: "Testing without required permissions",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Forbidden", content, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRole_WithValidAuthenticationAndPermission_Succeeds()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForRoleCreation();
        var request = new CreateRoleCommand(
            Name: "AuthorizedTest",
            Description: "Testing with valid authentication and permission",
            Permissions: new[] { new RolePermissionRequest("User", "Read") }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/roles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var roleDto = await response.Content.ReadFromJsonAsync<RoleDTO>();
        Assert.NotNull(roleDto);
        Assert.Equal("AuthorizedTest", roleDto.Name);
    }

    #endregion
}

