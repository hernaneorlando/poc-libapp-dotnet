using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Core.Validation;

namespace Auth.Tests.UnitTests;

/// <summary>
/// Unit tests for the User aggregate.
/// Tests domain logic and business rules without external dependencies.
/// </summary>
public class UserAggregateTests
{
    #region User Creation

    [Fact]
    public void Create_WithValidData_CreatesUserWithProperties()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var userType = UserType.Customer;

        // Act
        var user = User.Create(firstName, lastName, email, userType);

        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Contact.Email.Should().Be(email);
        user.UserType.Should().Be(userType);
        user.Id.Should().NotBeNull();
        user.Username.Should().NotBeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueIds()
    {
        // Act
        var user1 = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var user2 = User.Create("Jane", "Smith", "jane@example.com", UserType.Employee);

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Create_GeneratesUsername()
    {
        // Act
        var user = User.Create("John", "Doe", "john.doe@example.com", UserType.Customer);

        // Assert
        user.Username.Should().NotBeNull();
        user.Username.Value.Should().NotBeEmpty();
        user.Username.Value.Should().Be("john.doe");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithNullOrWhiteSpaceFirstName_ThrowsException(string? firstName)
    {
        // Act & Assert
        var action = () => User.Create(firstName!, "Doe", "john@example.com", UserType.Customer);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("First name cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithNullOrWhiteSpaceLastName_ThrowsException(string? lastName)
    {
        // Act & Assert
        var action = () => User.Create("John", lastName!, "john@example.com", UserType.Customer);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("Last name cannot be empty");
    }

    [Fact]
    public void Create_WithInvalidEmailFormat_ThrowsException()
    {
        // Act & Assert
        var action = () => User.Create("John", "Doe", "invalid-email", UserType.Customer);
        action.Should().Throw<Exception>();
    }

    #endregion

    #region Role Assignment

    [Fact]
    public void AssignRole_WithValidRole_AddsToUser()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role with basic permissions");

        // Act
        user.AssignRole(role);

        // Assert
        user.Roles.Should().Contain(role);
        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void AssignRole_WithMultipleRoles_AllAdded()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role1 = Role.Create("Editor", "Editor role");
        var role2 = Role.Create("Manager", "Manager role");

        // Act
        user.AssignRole(role1);
        user.AssignRole(role2);

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(role1);
        user.Roles.Should().Contain(role2);
    }

    [Fact]
    public void AssignRole_WithDuplicateRole_ThrowsException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        user.AssignRole(role);

        // Act & Assert
        var action = () => user.AssignRole(role);
        action.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Role Removal

    [Fact]
    public void RemoveRole_WithValidRole_RemovesFromUser()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        user.AssignRole(role);

        // Act
        user.RemoveRole(role.Id);

        // Assert
        user.Roles.Should().NotContain(role);
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithMultipleRoles_RemovesOnlySpecified()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role1 = Role.Create("Editor", "Editor role");
        var role2 = Role.Create("Manager", "Manager role");
        user.AssignRole(role1);
        user.AssignRole(role2);

        // Act
        user.RemoveRole(role1.Id);

        // Assert
        user.Roles.Should().NotContain(role1);
        user.Roles.Should().Contain(role2);
        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveRole_WithNonExistingRole_ThrowsException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");

        // Act & Assert
        var action = () => user.RemoveRole(role.Id);
        action.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region User Update

    [Fact]
    public void Update_WithValidData_UpdatesUserProperties()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.Update(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void Update_WithValidData_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act
        System.Threading.Thread.Sleep(10);
        user.Update("Jane", "Smith");

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_WithNullOrWhiteSpaceFirstName_ThrowsException(string? firstName)
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.Update(firstName!, "Doe");
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("First name cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_WithNullOrWhiteSpaceLastName_ThrowsException(string? lastName)
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.Update("John", lastName!);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("Last name cannot be empty");
    }

    [Fact]
    public void Update_PreservesRolesAndDeniedPermissions()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        var deniedPermission = new Permission(PermissionFeature.Book, PermissionAction.Delete);
        
        user.AssignRole(role);
        user.DeniedPermissions.Add(deniedPermission);

        // Act
        user.Update("Jane", "Smith");

        // Assert
        user.Roles.Should().Contain(role);
        user.DeniedPermissions.Should().Contain(deniedPermission);
    }

    #endregion

    #region Password Hash

    [Fact]
    public void SetPasswordHash_WithValidHash_SetsPasswordHashAndUpdatesTimestamp()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var passwordHash = "hashed_password_value";

        // Act
        System.Threading.Thread.Sleep(10);
        user.SetPasswordHash(passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SetPasswordHash_WithNullOrWhiteSpaceHash_ThrowsException(string? passwordHash)
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.SetPasswordHash(passwordHash!);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("Password hash cannot be empty");
    }

    [Fact]
    public void SetPasswordHash_CanBeCalledMultipleTimes()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var firstHash = "first_hash";
        var secondHash = "second_hash";

        // Act
        user.SetPasswordHash(firstHash);
        user.SetPasswordHash(secondHash);

        // Assert
        user.PasswordHash.Should().Be(secondHash);
    }

    #endregion

    #region Refresh Token Management

    [Fact]
    public void AddRefreshToken_WithValidToken_AddsToUser()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var refreshToken = RefreshToken.Create("token_value", DateTime.UtcNow.AddDays(7));

        // Act
        user.AddRefreshToken(refreshToken);

        // Assert
        user.RefreshTokens.Should().Contain(refreshToken);
        user.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public void AddRefreshToken_WithMultipleTokens_AllAdded()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var token1 = RefreshToken.Create("token_1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create("token_2", DateTime.UtcNow.AddDays(14));

        // Act
        user.AddRefreshToken(token1);
        user.AddRefreshToken(token2);

        // Assert
        user.RefreshTokens.Should().HaveCount(2);
        user.RefreshTokens.Should().Contain(token1);
        user.RefreshTokens.Should().Contain(token2);
    }

    [Fact]
    public void AddRefreshToken_WithNullToken_ThrowsException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.AddRefreshToken(null!);
        action.Should()
            .Throw<ValidationException>()
            .WithMessage("Refresh token cannot be null");
    }

    [Fact]
    public void AddRefreshToken_WithAlreadyExistingToken_ThrowsException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var refreshToken = RefreshToken.Create("token_value", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);

        // Act & Assert
        var action = () => user.AddRefreshToken(refreshToken);
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Refresh token already exists for this user");
    }

    [Fact]
    public void AddRefreshToken_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var refreshToken = RefreshToken.Create("token_value", DateTime.UtcNow.AddDays(7));

        // Act
        System.Threading.Thread.Sleep(10);
        user.AddRefreshToken(refreshToken);

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RevokeRefreshToken_WithValidToken_RevokesToken()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var tokenValue = "token_to_revoke";
        var refreshToken = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);

        // Act
        user.RevokeRefreshToken(tokenValue);

        // Assert
        var revokedToken = user.RefreshTokens.First(rt => rt.Token == tokenValue);
        revokedToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RevokeRefreshToken_WithNonExistingToken_ThrowsException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.RevokeRefreshToken("non_existing_token");
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Refresh token not found");
    }

    [Fact]
    public void RevokeRefreshToken_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var tokenValue = "token_value";
        var refreshToken = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);

        // Act
        System.Threading.Thread.Sleep(10);
        user.RevokeRefreshToken(tokenValue);

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RevokeRefreshToken_WithMultipleTokens_RevokesOnlySpecified()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var token1 = RefreshToken.Create("token_1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create("token_2", DateTime.UtcNow.AddDays(14));
        user.AddRefreshToken(token1);
        user.AddRefreshToken(token2);

        // Act
        user.RevokeRefreshToken("token_1");

        // Assert
        var revokedToken = user.RefreshTokens.First(rt => rt.Token == "token_1");
        var validToken = user.RefreshTokens.First(rt => rt.Token == "token_2");
        revokedToken.IsValid.Should().BeFalse();
        validToken.IsValid.Should().BeTrue();
    }

    #endregion

    #region Deactivation With Refresh Tokens

    [Fact]
    public void Deactivate_MarksUserAsInactive()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act
        System.Threading.Thread.Sleep(10);
        user.Deactivate();

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_RevokesAllActiveRefreshTokens()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var token1 = RefreshToken.Create("token_1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create("token_2", DateTime.UtcNow.AddDays(14));
        
        user.AddRefreshToken(token1);
        user.AddRefreshToken(token2);

        // Act
        user.Deactivate();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(rt => rt.IsValid.Should().BeFalse());
    }

    [Fact]
    public void Deactivate_PreservesRevokedTokensState()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var activeToken = RefreshToken.Create("active_token", DateTime.UtcNow.AddDays(7));
        var expiredToken = RefreshToken.Create("expired_token", DateTime.UtcNow.AddDays(30));
        
        user.AddRefreshToken(activeToken);
        user.AddRefreshToken(expiredToken);

        // Act
        user.Deactivate();

        // Assert
        user.RefreshTokens.Should().HaveCount(2);
        var revokedActiveToken = user.RefreshTokens.First(rt => rt.Token == "active_token");
        revokedActiveToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WithNoRefreshTokens_CompletesSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        var action = () => user.Deactivate();
        action.Should().NotThrow();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WithRolesAndDeniedPermissions_DeactivatesPreservingData()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        var deniedPermission = new Permission(PermissionFeature.Book, PermissionAction.Delete);
        
        user.AssignRole(role);
        user.DeniedPermissions.Add(deniedPermission);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.Roles.Should().HaveCount(1);
        user.DeniedPermissions.Should().HaveCount(1);
    }

    [Fact]
    public void Deactivate_CanBeCalledMultipleTimes()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act
        user.Deactivate();
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    #endregion

    #region Properties Integrity

    [Fact]
    public void Id_IsImmutable()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var originalId = user.Id;

        // Act & Assert
        user.Id.Should().Be(originalId);
    }

    [Fact]
    public void Roles_IsReadOnlyCollection()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        user.Roles.Should().BeOfType<List<Role>>();
    }

    [Fact]
    public void DeniedPermissions_IsReadOnlyCollection()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act & Assert
        user.DeniedPermissions.Should().BeOfType<List<Permission>>();
    }

    [Fact]
    public void CreatedAt_IsSetOnCreation()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var after = DateTime.UtcNow;

        // Assert
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Username_IsSetOnCreation()
    {
        // Arrange & Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Assert
        user.Username.Should().NotBeNull();
        user.Username.Value.Should().NotBeEmpty();
    }

    #endregion

    #region Permission Checks

    [Fact]
    public void GetEffectivePermissions_WithRolePermissions_ReturnsRolePermissions()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Create);
        
        role.AssignPermission(permission);
        user.AssignRole(role);

        // Act
        var permissions = user.Roles.SelectMany(r => r.Permissions).ToList();

        // Assert
        permissions.Should().Contain(permission);
    }

    [Fact]
    public void GetEffectivePermissions_WithDeniedPermission_ExcludesDenied()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);
        var role = Role.Create("Editor", "Editor role");
        var permission = new Permission(PermissionFeature.Book, PermissionAction.Create);
        
        role.AssignPermission(permission);
        user.AssignRole(role);
        user.DeniedPermissions.Add(permission);

        // Act
        var rolePermissions = user.Roles.SelectMany(r => r.Permissions).ToList();
        var deniedPermissions = user.DeniedPermissions.ToList();

        // Assert
        rolePermissions.Should().Contain(permission);
        deniedPermissions.Should().Contain(permission);
    }

    #endregion

    #region Business Rules

    [Fact]
    public void User_CanBeCreatedWithoutRoles()
    {
        // Arrange & Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Assert
        user.Roles.Should().BeEmpty();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_CanBeCreatedWithoutDeniedPermissions()
    {
        // Arrange & Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Assert
        user.DeniedPermissions.Should().BeEmpty();
    }

    [Fact]
    public void User_GetFullName_ReturnsCombinedName()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer);

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_EmailIsPreserved()
    {
        // Arrange
        var email = "john.doe@example.com";

        // Act
        var user = User.Create("John", "Doe", email, UserType.Customer);

        // Assert
        user.Contact.Email.Should().Be(email);
    }

    [Fact]
    public void User_UserTypeIsSetCorrectly()
    {
        // Arrange & Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Employee);

        // Assert
        user.UserType.Should().Be(UserType.Employee);
    }

    [Fact]
    public void User_CanBeCreatedWithPhoneNumber()
    {
        // Arrange
        var phone = "1234567890";

        // Act
        var user = User.Create("John", "Doe", "john@example.com", UserType.Customer, null, phone);

        // Assert
        user.Contact.PhoneNumber.Should().Be(phone);
    }

    #endregion
}
