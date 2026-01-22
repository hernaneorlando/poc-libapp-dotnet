using Auth.Domain.Aggregates.Role;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Enums;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Auth.Tests.IntegrationTests.RepositoryTests.Roles;

/// <summary>
/// Integration tests for the RoleRepository.
/// Tests persistence operations and query methods with a real in-memory database.
/// Validates EF Core mappings and database interactions without the Application/Handler layer.
/// </summary>
public class RoleRepositoryIntegrationTests : IAsyncLifetime
{
    private AuthDbContext _dbContext = null!;
    private RoleRepository _roleRepository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AuthDbContext(options);
        _roleRepository = new RoleRepository(_dbContext);

        await _dbContext.Database.EnsureCreatedAsync();
    }

    #region Add/Create Operations

    [Fact]
    public async Task AddAsync_ValidRole_PersistsSuccessfully()
    {
        // Arrange
        var name = "Administrator";
        var role = Role.Create(name, "Full system access");

        // Act
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedRole = await _dbContext.Roles.FindAsync(role.Id.Value);
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be(name);
    }

    [Fact]
    public async Task AddAsync_RoleWithPermissions_PersistsWithPermissions()
    {
        // Arrange
        var name = "Editor";
        var description = "Can edit content";
        var role = Role.Create(name, description);
        var permission1 = new Permission(
            PermissionFeature.Book,
            PermissionAction.Update
        );
        var permission2 = new Permission(
            PermissionFeature.Book,
            PermissionAction.Read
        );
        role.AssignPermission(permission1);
        role.AssignPermission(permission2);

        // Act
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedRoleEntity = await _dbContext.Roles.FirstAsync(r => r.Id == role.Id.Value);
        
        savedRoleEntity.Name.Should().Be(name);
        savedRoleEntity.Description.Should().Contain(description);
        savedRoleEntity.PermissionsJson.Should().NotBeNullOrEmpty();
        
        var savedRole = (Role)savedRoleEntity;
        savedRole.Permissions.Should().HaveCount(2);
    }

    #endregion

    #region Get/Read Operations

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsRole()
    {
        // Arrange
        var role = Role.Create("Viewer", "Read-only access");
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRole = await _roleRepository.GetByIdAsync(role.Id);

        // Assert
        retrievedRole.Should().NotBeNull();
        retrievedRole!.Name.Should().Be("Viewer");
        retrievedRole.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingRole_ReturnsNull()
    {
        // Arrange
        var nonExistingId = RoleId.New();

        // Act
        var retrievedRole = await _roleRepository.GetByIdAsync(nonExistingId);

        // Assert
        retrievedRole.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingRole_ReturnsRole()
    {
        // Arrange
        var name = "Manager";
        var role = Role.Create(name, "Management access");
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedRole = await _roleRepository.GetByNameAsync(name);

        // Assert
        retrievedRole.Should().NotBeNull();
        retrievedRole!.Name.Should().Be(name);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingRole_ReturnsNull()
    {
        // Act
        var retrievedRole = await _roleRepository.GetByNameAsync("NonExistentRole");

        // Assert
        retrievedRole.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var role1 = Role.Create("Admin", "Administrator");
        var role2 = Role.Create("User", "Standard user");
        var role3 = Role.Create("Guest", "Guest access");

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        // Act
        var allRoles = await _roleRepository.GetAllAsync();

        // Assert
        allRoles.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var allRoles = await _roleRepository.GetAllAsync();

        // Assert
        allRoles.Should().BeEmpty();
    }

    #endregion

    #region Update Operations

    [Fact]
    public async Task UpdateAsync_ExistingRole_UpdatesSuccessfully()
    {
        // Arrange
        var role = Role.Create("Operator", "Operator role");
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        // Clear the change tracker to simulate a fresh context
        _dbContext.ChangeTracker.Clear();

        // Re-fetch the role
        var roleToUpdate = await _roleRepository.GetByIdAsync(role.Id);
        var name = "Senior Operator";
        var description = "Senior operator with extended access";
        roleToUpdate!.Update(name, description);

        // Act
        await _roleRepository.UpdateAsync(roleToUpdate);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker again
        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedRole = await _roleRepository.GetByIdAsync(role.Id);
        updatedRole!.Name.Should().Be(name);
        updatedRole!.Description.Should().Be(description);
    }

    [Fact]
    public async Task UpdateAsync_RoleWithPermissions_UpdatesWithoutLosingPermissions()
    {
        // Arrange
        var role = Role.Create("Editor", "Editor role");
        var permission = new Permission(
            PermissionFeature.Book,
            PermissionAction.Create
        );
        
        role.AssignPermission(permission);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var roleToUpdate = await _roleRepository.GetByIdAsync(role.Id);
        var name = "Senior Editor";
        var description = "Senior editor with more access";
        roleToUpdate!.Update(name, description);

        // Act
        await _roleRepository.UpdateAsync(roleToUpdate);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedRole = await _roleRepository.GetByIdAsync(role.Id);
        
        updatedRole.Should().NotBeNull();
        updatedRole!.Name.Should().Be(name);
        updatedRole.Description.Should().Be(description);
        updatedRole.Permissions.Should().HaveCount(1);
    }

    #endregion

    #region Delete Operations

    [Fact]
    public async Task DeleteAsync_ExistingRole_DeletesSuccessfully()
    {
        // Arrange
        var role = Role.Create("Temporary", "Temporary role");
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();
        var roleId = role.Id;

        // Act
        await _roleRepository.DeleteAsync(roleId);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedRole = await _roleRepository.GetByIdAsync(roleId);
        deletedRole.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RoleWithPermissions_DeletesRoleAndPermissions()
    {
        // Arrange
        var role = Role.Create("ToDelete", "Role to delete");
        var permission = new Permission(
            PermissionFeature.User,
            PermissionAction.Read
        );
        role.AssignPermission(permission);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();
        var roleId = role.Id;

        // Act
        await _roleRepository.DeleteAsync(roleId);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedRole = await _roleRepository.GetByIdAsync(roleId);
        
        deletedRole.Should().BeNull();
    }

    #endregion

    #region Query Scenarios

    [Fact]
    public async Task GetAllAsync_WithMultipleRoles_FiltersCorrectly()
    {
        // Arrange
        var activeRole = Role.Create("Active", "Active role");
        var inactiveRole = Role.Create("Inactive", "Inactive role");
        inactiveRole.Deactivate();

        await _roleRepository.AddAsync(activeRole);
        await _roleRepository.AddAsync(inactiveRole);
        await _dbContext.SaveChangesAsync();

        // Act
        var allRoles = (await _roleRepository.GetAllAsync()).ToList();

        // Assert
        // GetAllAsync filters by IsActive = true, so only active roles are returned
        allRoles.Should().HaveCount(1);
        allRoles[0].Name.Should().Be("Active");
        allRoles[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_LoadsRelatedPermissions()
    {
        // Arrange
        var role = Role.Create("WithPerms", "Role with permissions");
        var perm1 = new Permission(
            PermissionFeature.Book,
            PermissionAction.Create
        );
        var perm2 = new Permission(
            PermissionFeature.User,
            PermissionAction.Delete
        );
        role.AssignPermission(perm1);
        role.AssignPermission(perm2);

        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var retrievedRole = await _roleRepository.GetByIdAsync(role.Id);

        // Assert
        retrievedRole.Should().NotBeNull();
        retrievedRole!.Permissions.Should().HaveCount(2);
    }

    #endregion

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
    }
}
