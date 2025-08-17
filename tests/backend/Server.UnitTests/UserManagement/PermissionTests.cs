using Domain.UserManagement;
using Domain.UserManagement.Enums;

namespace Server.UnitTests.UserManagement;

public class PermissionTests
{
    private const string StringEmpty = "";
    private const string WhiteSpace = " ";

    [Theory]
    [InlineData("Authorization to update permission.")]
    [InlineData(StringEmpty)]
    [InlineData(null)]
    public void CreatePermission_ValidInputs_ReturnsPermission(string? description)
    {
        // Arrange
        var validCode = $"{PermissionFeature.Permission}:{PermissionAction.Update}";

        // Act
        var result = Permission.Create(validCode, description!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(description, result.Value.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StringEmpty)]
    [InlineData(WhiteSpace)]
    public void CreatePermission_NoCode_ReturnsError(string? invalidCode)
    {
        // Arrange
        // Act
        var result = Permission.Create(invalidCode!, "Some description");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Permission cannot be created without code.", result.Errors);
    }

    [Fact]
    public void CreatePermission_InvalidCodeFormat_ReturnError()
    {
        // Arrange
        var invalidCode = "random_text";

        // Act
        var result = Permission.Create(invalidCode!, "Some description");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid Permission code format", result.Errors);
    }

    [Fact]
    public void CreatePermission_InvalidCode_ReturnError()
    {
        // Arrange
        var invalidCode = "invalid:code";

        // Act
        var result = Permission.Create(invalidCode!, "Some description");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Error creating Permission due to non-existent feature.", result.Errors);
        Assert.Contains("Error creating Permission due to non-existent action.", result.Errors);
        Assert.Null(result.Value);
    }

    [Fact]
    public void CreatePermission_DescriptionTooLong_ReturnsError()
    {
        // Arrange
        var validCode = $"{PermissionFeature.Permission}:{PermissionAction.Update}";
        var invalidName = new string('a', 257);
        // Act
        var result = Permission.Create(validCode, invalidName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Permission Description must not exceed 256 characters", result.Errors);
    }

    [Fact]
    public void UpdatePermission_ValidInputs_ReturnsUpdatedPermission()
    {
        // Arrange
        var permissionExternalId = Guid.NewGuid();
        var permission = Permission.Create(
            PermissionFeature.Permission,
            PermissionAction.Update,
            "Permission feature update");

        permission.ExternalId = permissionExternalId;

        const string newDescription = "Authorization to update permission";

        // Act
        var result = permission.Update(permissionExternalId, newDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newDescription, result.Value.Description);
    }

    [Fact]
    public void UpdatePermission_NullDescription_ReturnsSuccess()
    {
        // Arrange
        var permissionExternalId = Guid.NewGuid();
        var permission = Permission.Create(
            PermissionFeature.Permission,
            PermissionAction.Update,
            "Permission feature update");

        permission.ExternalId = permissionExternalId;

        // Act
        var result = permission.Update(permissionExternalId, null!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(permission.Description);
    }

    [Fact]
    public void UpdatePermission_InvalidInputs_ReturnsError()
    {
        // Arrange
        var permission = Permission.Create(
            PermissionFeature.Permission,
            PermissionAction.Update,
            "Permission feature update");

        permission.ExternalId = Guid.NewGuid();

        // Act
        var result1 = permission.Update(
            permission.ExternalId,
            new('a', 257));

        var result2 = permission.Update(
            Guid.NewGuid(),
            "Some description");

        // Assert
        Assert.True(result1.IsFailure);
        Assert.True(result2.IsFailure);
        Assert.Contains("Permission Description must not exceed 256 characters", result1.Errors);
        Assert.Contains("Permissions must have the same External Id", result2.Errors);
    }
}
