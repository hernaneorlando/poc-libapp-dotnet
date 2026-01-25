using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Enums;
using Core.Validation;

namespace Auth.Tests.UnitTests;

/// <summary>
/// Unit tests for the Role aggregate.
/// Tests domain logic and business rules without external dependencies.
/// </summary>
public class RoleAggregateTests
{
    #region Role Creation

    [Fact]
    public void Create_WithValidData_CreatesRoleWithProperties()
    {
        // Arrange
        var name = "Administrator";
        var description = "Administrator role with full access";
        
        // Act
        var role = Role.Create(name, description);

        // Assert
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);
        role.Id.Should().NotBeNull();
        role.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueIds()
    {
        // Act
        var role1 = Role.Create("Admin1", "First admin role");
        var role2 = Role.Create("Admin2", "Second admin role");

        // Assert
        role1.Id.Should().NotBe(role2.Id);
    }

    #endregion

    #region Permission Assignment

    [Fact]
    public void AssignPermission_WithValidPermission_AddsToRole()
    {
        // Arrange
        var role = Role.Create("Editor", "Editor role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Create);

        // Act
        role.AssignPermission(permission);

        // Assert
        role.Permissions.Should().Contain(permission);
        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void AssignPermission_WithMultiplePermissions_AllAdded()
    {
        // Arrange
        var role = Role.Create("Manager", "Manager role");
        var permissions = new[]
        {
            new Permission(PermissionFeature.User, PermissionAction.Create),
            new Permission(PermissionFeature.User, PermissionAction.Read),
            new Permission(PermissionFeature.Book, PermissionAction.Update)
        };

        // Act
        role.AssignPermission(permissions);

        // Assert
        role.Permissions.Should().HaveCount(3);
    }

    [Fact]
    public void AssignPermission_WithDuplicate_ThrowsException()
    {
        // Arrange
        var role = Role.Create("Viewer", "Viewer role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Read);

        role.AssignPermission(permission);

        // Act & Assert
        var action = () => role.AssignPermission(permission);
        action.Should().Throw<ValidationException>();
    }

    [Fact]
    public void AssignPermission_PreservesExistingPermissions()
    {
        // Arrange
        var role = Role.Create("Operator", "Operator role");
        var perm1 = new Permission(PermissionFeature.User, PermissionAction.Read);
        var perm2 = new Permission(PermissionFeature.Book, PermissionAction.Create);

        role.AssignPermission(perm1);

        // Act
        role.AssignPermission(perm2);

        // Assert
        role.Permissions.Should().Contain(perm1);
        role.Permissions.Should().Contain(perm2);
        role.Permissions.Should().HaveCount(2);
    }

    #endregion

    #region Permission Removal

    [Fact]
    public void RemovePermission_WithValidPermission_RemovesFromRole()
    {
        // Arrange
        var role = Role.Create("Editor", "Editor role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Create);
        role.AssignPermission(permission);

        // Act
        role.RemovePermission(permission);

        // Assert
        role.Permissions.Should().NotContain(permission);
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void RemovePermission_WithMultiplePermissions_RemovesOnlySpecified()
    {
        // Arrange
        var role = Role.Create("Manager", "Manager role");
        var perm1 = new Permission(PermissionFeature.User, PermissionAction.Create);
        var perm2 = new Permission(PermissionFeature.Book, PermissionAction.Read);
        role.AssignPermission(perm1);
        role.AssignPermission(perm2);

        // Act
        role.RemovePermission(perm1);

        // Assert
        role.Permissions.Should().NotContain(perm1);
        role.Permissions.Should().Contain(perm2);
        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void RemovePermission_WithNullPermission_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Viewer", "Viewer role");

        // Act & Assert
        var action = () => role.RemovePermission(null!);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("*Permission cannot be null*");
    }

    [Fact]
    public void RemovePermission_WithNonExistingPermission_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Operator", "Operator role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Read);

        // Act & Assert
        var action = () => role.RemovePermission(permission);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Permission not assigned*");
    }

    [Fact]
    public void RemovePermission_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var role = Role.Create("Admin", "Admin role");
        var permission = new Permission(PermissionFeature.User, PermissionAction.Create);
        role.AssignPermission(permission);

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure the time has changed
        role.RemovePermission(permission);

        // Assert
        role.UpdatedAt.Should().NotBeNull();
        role.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Role Update

    [Fact]
    public void Update_WithValidData_UpdatesRoleProperties()
    {
        // Arrange
        var role = Role.Create("Admin", "Admin role");
        var newName = "SuperAdmin";
        var newDescription = "Super Administrator role with full access";

        // Act
        role.Update(newName, newDescription);

        // Assert
        role.Name.Should().Be(newName);
        role.Description.Should().Be(newDescription);
        role.Id.Should().NotBeNull(); // ID must remain the same
    }

    [Fact]
    public void Update_WithValidData_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var role = Role.Create("Editor", "Editor role");

        // Act
        System.Threading.Thread.Sleep(10); // Pequeno delay para garantir que o tempo mudou
        role.Update("New Editor", "Updated editor role");

        // Assert
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNullName_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update(null!, "Updated description");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*empty*");
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update(string.Empty, "Updated description");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*empty*");
    }

    [Fact]
    public void Update_WithWhitespaceName_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update("   ", "Updated description");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*empty*");
    }

    [Fact]
    public void Update_WithNullDescription_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update("Updated", null!);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*empty*");
    }

    [Fact]
    public void Update_WithEmptyDescription_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update("Updated", string.Empty);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*empty*");
    }

    [Fact]
    public void Update_WithWhitespaceDescription_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        var action = () => role.Update("Updated", "   ");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*empty*");
    }

    [Fact]
    public void Update_PreservesPermissions()
    {
        // Arrange
        var role = Role.Create("Manager", "Manager role");
        var permission = new Permission(PermissionFeature.User, PermissionAction.Read);
        role.AssignPermission(permission);

        // Act
        role.Update("Senior Manager", "Senior manager role");

        // Assert
        role.Permissions.Should().Contain(permission);
        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void Update_TrimsWhitespace()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act
        role.Update("  New Name  ", "  New Description  ");

        // Assert
        role.Name.Should().Be("New Name");
        role.Description.Should().Be("New Description");
    }

    #endregion

    #region Permission Queries

    [Fact]
    public void HasPermission_WithExistingPermission_ReturnsTrue()
    {
        // Arrange
        var role = Role.Create("Admin", "Admin role");
        var permission = new Permission(PermissionFeature.User, PermissionAction.Create);
        role.AssignPermission(permission);

        // Act
        var hasPermission = role.Permissions.Contains(permission);

        // Assert
        hasPermission.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithNonExistingPermission_ReturnsFalse()
    {
        // Arrange
        var role = Role.Create("Viewer", "Viewer role");
        var permission = new Permission(PermissionFeature.User, PermissionAction.Create);

        // Act
        var hasPermission = role.Permissions.Contains(permission);

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact]
    public void FilterPermissionsByFeature_ReturnsOnlyMatchingFeatures()
    {
        // Arrange
        var role = Role.Create("Mixed", "Mixed permissions");
        role.AssignPermission(new Permission(PermissionFeature.User, PermissionAction.Create));
        role.AssignPermission(new Permission(PermissionFeature.User, PermissionAction.Read));
        role.AssignPermission(new Permission(PermissionFeature.Book, PermissionAction.Create));

        // Act
        var userPermissions = role.Permissions.Where(p => p.Feature == PermissionFeature.User).ToList();

        // Assert
        userPermissions.Should().HaveCount(2);
        userPermissions.Should().AllSatisfy(p => p.Feature.Should().Be(PermissionFeature.User));
    }

    #endregion

    #region Properties Integrity

    [Fact]
    public void Id_IsImmutable()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");
        var originalId = role.Id;

        // Act & Assert
        role.Id.Should().Be(originalId);
    }

    [Fact]
    public void Permissions_IsReadOnlyCollection()
    {
        // Arrange
        var role = Role.Create("Test", "Test role description");

        // Act & Assert
        role.Permissions.Should().BeAssignableTo<IReadOnlyCollection<Permission>>();
    }

    [Fact]
    public void CreatedAt_IsSetOnCreation()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var role = Role.Create("Test", "Test role description");
        var after = DateTime.UtcNow;

        // Assert
        role.CreatedAt.Should().BeOnOrAfter(before);
        role.CreatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Business Rules

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithNullOrWhiteSpaceName_ThrowsValidationException(string? name)
    {
        // Act & Assert
        var action = () => Role.Create(name!, "Test description");
        action.Should().Throw<ValidationException>()
            .WithMessage("Name cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithNullOrWhiteSpaceDescription_ThrowsValidationException(string? description)
    {
        // Act & Assert
        var action = () => Role.Create("Admin", description!);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*empty*");
    }

    [Fact]
    public void Create_WithNameTooShort_ThrowsValidationException()
    {
        // Act & Assert
        var action = () => Role.Create("AB", "Valid description for testing");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*at least 3 characters*");
    }

    [Fact]
    public void Create_WithNameTooLong_ThrowsValidationException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act & Assert
        var action = () => Role.Create(longName, "Valid description for testing");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*not exceed 50 characters*");
    }

    [Fact]
    public void Create_WithDescriptionTooShort_ThrowsValidationException()
    {
        // Act & Assert
        var action = () => Role.Create("Admin", "Short");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*at least 10 characters*");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ThrowsValidationException()
    {
        // Arrange
        var longDescription = new string('A', 501);

        // Act & Assert
        var action = () => Role.Create("Admin", longDescription);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*not exceed 256 characters*");
    }

    [Fact]
    public void Update_WithNameTooShort_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Admin", "Valid description for testing");

        // Act & Assert
        var action = () => role.Update("AB", "Updated description");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*at least 3 characters*");
    }

    [Fact]
    public void Update_WithNameTooLong_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Admin", "Valid description for testing");
        var longName = new string('A', 51);

        // Act & Assert
        var action = () => role.Update(longName, "Updated description");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Name*not exceed 50 characters*");
    }

    [Fact]
    public void Update_WithDescriptionTooShort_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Admin", "Valid description for testing");

        // Act & Assert
        var action = () => role.Update("UpdatedAdmin", "Short");
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*at least 10 characters*");
    }

    [Fact]
    public void Update_WithDescriptionTooLong_ThrowsValidationException()
    {
        // Arrange
        var role = Role.Create("Admin", "Valid description for testing");
        var longDescription = new string('A', 257);

        // Act & Assert
        var action = () => role.Update("UpdatedAdmin", longDescription);
        action.Should().Throw<ValidationException>()
            .WithMessage("*Description*not exceed 256 characters*");
    }

    [Fact]
    public void Role_ValidWithoutPermissions()
    {
        // Arrange & Act
        var role = Role.Create("Guest", "Guest role");

        // Assert
        role.Name.Should().Be("Guest");
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Role_RemainsActiveAfterPermissionAssignment()
    {
        // Arrange
        var role = Role.Create("Active", "Active role");
        var permission = new Permission(PermissionFeature.User, PermissionAction.Read);

        // Act
        role.AssignPermission(permission);

        // Assert
        role.Name.Should().Be("Active");
        role.Permissions.Should().HaveCount(1);
    }

    #endregion

    #region Role Deactivation

    [Fact]
    public void Deactivate_MarksRoleAsInactive()
    {
        // Arrange
        var role = Role.Create("Admin", "Admin role");

        // Act
        role.Deactivate();

        // Assert
        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var role = Role.Create("Editor", "Editor role");
        var updatedAtBefore = DateTime.UtcNow;

        // Act
        role.Deactivate();
        var updatedAtAfter = DateTime.UtcNow;

        // Assert
        role.UpdatedAt.Should().NotBeNull();
        role.UpdatedAt.Should().BeOnOrAfter(updatedAtBefore);
        role.UpdatedAt.Should().BeOnOrBefore(updatedAtAfter);
    }

    [Fact]
    public void Deactivate_WithPermissions_DeactivatesWithPermissions()
    {
        // Arrange
        var role = Role.Create("Manager", "Manager role");
        role.AssignPermission(new Permission(PermissionFeature.User, PermissionAction.Read));
        role.AssignPermission(new Permission(PermissionFeature.Book, PermissionAction.Create));

        // Act
        role.Deactivate();

        // Assert
        role.IsActive.Should().BeFalse();
        role.Permissions.Should().HaveCount(2); // Permissions are preserved
    }

    [Fact]
    public void Deactivate_CanBeCalledMultipleTimes()
    {
        // Arrange
        var role = Role.Create("Viewer", "Viewer role");

        // Act
        role.Deactivate();
        role.Deactivate(); // Deactivate again

        // Assert
        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_PreservesNameAndDescription()
    {
        // Arrange
        var role = Role.Create("Operator", "Operator role");
        var originalName = role.Name;
        var originalDescription = role.Description;

        // Act
        role.Deactivate();

        // Assert
        role.Name.Should().Be(originalName);
        role.Description.Should().Be(originalDescription);
    }

    #endregion
}
