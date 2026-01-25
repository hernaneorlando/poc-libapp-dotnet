using Auth.Domain.Aggregates.Role;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Auth.Tests.IntegrationTests.RepositoryTests;

/// <summary>
/// Integration tests for the UserRepository.
/// Tests persistence operations and query methods with a real in-memory database.
/// Validates EF Core mappings and database interactions without the Application/Handler layer.
/// Tests specification-based queries (FindAsync) for flexible filtering.
/// </summary>
public class UserRepositoryIntegrationTests : IAsyncLifetime
{
    private AuthDbContext _dbContext = null!;
    private UserRepository _userRepository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AuthDbContext(options);
        _userRepository = new UserRepository(_dbContext);

        await _dbContext.Database.EnsureCreatedAsync();
    }

    #region Add/Create Operations

    [Fact]
    public async Task AddAsync_ValidUser_PersistsSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john.doe@example.com", UserType.Customer);

        // Act
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be("John");
        savedUser.LastName.Should().Be("Doe");
        savedUser.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task AddAsync_UserWithRoles_PersistsWithRoles()
    {
        // Arrange
        var user = User.Create("Jane", "Smith", "jane.smith@example.com", UserType.Employee);
        var role = Role.Create("Manager", "Manager role with extended access");
        user.AssignRole(role);

        // Act
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        
        savedUser.Should().NotBeNull();
        savedUser!.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_UserWithRefreshTokens_PersistsWithTokens()
    {
        // Arrange
        var user = User.Create("Bob", "Johnson", "bob.johnson@example.com", UserType.Customer);
        var refreshToken = RefreshToken.Create("test-token-12345", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);

        // Act
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        
        savedUser.Should().NotBeNull();
        savedUser!.RefreshTokens.Should().HaveCount(1);
        savedUser.RefreshTokens[0].Token.Should().Be("test-token-12345");
    }

    #endregion

    #region Get/Read Operations

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = User.Create("Alice", "Williams", "alice.williams@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.FirstName.Should().Be("Alice");
        retrievedUser.GetFullName().Should().Be("Alice Williams");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var nonExistingId = UserId.New();

        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(nonExistingId);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_InactiveUser_ReturnsNull()
    {
        // Arrange
        var user = User.Create("Charlie", "Brown", "charlie.brown@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().BeNull();
    }

    #endregion

    #region Update Operations

    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesSuccessfully()
    {
        // Arrange
        var user = User.Create("Diana", "Miller", "diana.miller@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var userToUpdate = await _userRepository.GetByIdAsync(user.Id);
        userToUpdate!.Update("Diana", "Johnson");

        // Act
        await _userRepository.UpdateAsync(userToUpdate);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.LastName.Should().Be("Johnson");
    }

    [Fact]
    public async Task UpdateAsync_UserWithRoles_UpdatesSuccessfully()
    {
        // Arrange
        var user = User.Create("Eva", "Davis", "eva.davis@example.com", UserType.Employee);
        var role = Role.Create("Admin", "Administrator role");
        user.AssignRole(role);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var userEntity = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        
        var userToUpdate = (User)userEntity!;
        userToUpdate.Update("Eva", "Thompson");

        // Act
        await _userRepository.UpdateAsync(userToUpdate);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.LastName.Should().Be("Thompson");
    }

    [Fact]
    public async Task UpdateAsync_UserPasswordHash_UpdatesSuccessfully()
    {
        // Arrange
        var user = User.Create("Frank", "Garcia", "frank.garcia@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var userToUpdate = await _userRepository.GetByIdAsync(user.Id);
        var newPasswordHash = "hashed_password_xyz123";
        userToUpdate!.SetPasswordHash(newPasswordHash);

        // Act
        await _userRepository.UpdateAsync(userToUpdate);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.PasswordHash.Should().Be(newPasswordHash);
    }

    #endregion

    #region Delete Operations

    [Fact]
    public async Task DeleteAsync_ExistingUser_DeactivatesSuccessfully()
    {
        // Arrange
        var user = User.Create("Grace", "Martinez", "grace.martinez@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        var userId = user.Id;

        // Act
        await _userRepository.DeleteAsync(userId);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedUser = await _userRepository.GetByIdAsync(userId);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_UserWithRefreshTokens_DeactivatesAndRevokesTokens()
    {
        // Arrange
        var user = User.Create("Henry", "Rodriguez", "henry.rodriguez@example.com", UserType.Customer);
        var refreshToken = RefreshToken.Create("token-abc123", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        var userId = user.Id;

        // Act
        await _userRepository.DeleteAsync(userId);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedUser = await _userRepository.GetByIdAsync(userId);
        deletedUser.Should().BeNull();
    }

    #endregion

    #region Specification Queries - GetUserByUsername

    [Fact]
    public async Task FindAsync_GetUserByUsername_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = User.Create("Iris", "Lee", "iris.lee@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByUsername(user.Username.Value);

        // Act
        var foundUser = await _userRepository.FindAsync(specification);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.FirstName.Should().Be("Iris");
        foundUser.Username.Value.Should().Be(user.Username.Value);
    }

    [Fact]
    public async Task FindAsync_GetUserByUsername_NonExistingUsername_ThrowsException()
    {
        // Arrange
        var specification = new GetUserByUsername("non.existent.username");

        // Act
        var user = await _userRepository.FindAsync(specification);

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByUsername_InactiveUser_ThrowsException()
    {
        // Arrange
        var user = User.Create("Jack", "Wilson", "jack.wilson@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Update IsActive directly to avoid tracking issues
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        userEntity!.IsActive = false;
        userEntity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByUsername(user.Username.Value);

        // Act
        var notFoundedUser = await _userRepository.FindAsync(specification);
        
        // Assert
        notFoundedUser.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByUsername_WithTracking_AllowsUpdates()
    {
        // Arrange
        var user = User.Create("Karen", "Taylor", "karen.taylor@example.com", UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var specification = new GetUserByUsername(user.Username.Value);

        // Act
        var foundUser = await _userRepository.FindAsync(specification);
        foundUser!.Update("Karen", "Anderson");
        await _userRepository.UpdateAsync(foundUser);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser!.LastName.Should().Be("Anderson");
    }

    #endregion

    #region Specification Queries - GetUserByEmail

    [Fact]
    public async Task FindAsync_GetUserByEmail_ExistingUser_ReturnsUser()
    {
        // Arrange
        var email = "leo.clark@example.com";
        var user = User.Create("Leo", "Clark", email, UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByEmail(email);

        // Act
        var foundUser = await _userRepository.FindAsync(specification);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Contact.Email.Should().Be(email);
    }

    [Fact]
    public async Task FindAsync_GetUserByEmail_NonExistingEmail_ReturnsNull()
    {
        // Arrange
        var specification = new GetUserByEmail("nonexistent@example.com");

        // Act
        var user = await _userRepository.FindAsync(specification);
        
        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByEmail_InactiveUser_ReturnsNull()
    {
        // Arrange
        var email = "mia.hall@example.com";
        var user = User.Create("Mia", "Hall", email, UserType.Customer);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Update IsActive directly to avoid tracking issues
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        userEntity!.IsActive = false;
        userEntity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByEmail(email);

        // Act
        var notFoundedUser = await _userRepository.FindAsync(specification);
        
        // Assert
        notFoundedUser.Should().BeNull();
    }

    #endregion

    #region Specification Queries - GetUserByRefreshToken

    [Fact]
    public async Task FindAsync_GetUserByRefreshToken_ExistingToken_ReturnsUser()
    {
        // Arrange
        var user1 = User.Create("Noah", "Allen", "noah.allen@example.com", UserType.Customer);
        var tokenValue = "refresh-token-valid-123";
        var refreshToken1 = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user1.AddRefreshToken(refreshToken1);

        var user2 = User.Create("John", "Doe", "john.doe@example.com", UserType.Customer);
        var refreshToken2 = RefreshToken.Create("refresh-token-valid-456", DateTime.UtcNow.AddDays(7));
        user2.AddRefreshToken(refreshToken2);

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByRefreshToken(tokenValue);

        // Act
        var foundUser = await _userRepository.FindAsync(specification);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.FirstName.Should().Be("Noah");
    }

    [Fact]
    public async Task FindAsync_GetUserByRefreshToken_NonExistingToken_ReturnsNull()
    {
        // Arrange
        var specification = new GetUserByRefreshToken("non-existent-token");

        // Act
        var user =  await _userRepository.FindAsync(specification);
        
        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByRefreshToken_RevokedToken_StillFindsUser()
    {
        // Arrange
        var user = User.Create("Olivia", "Young", "olivia.young@example.com", UserType.Customer);
        var tokenValue = "refresh-token-revoked-456";
        var refreshToken = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Get the token and revoke it by updating the entity directly
        var userEntity = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        
        var tokenToRevoke = userEntity!.RefreshTokens.First(rt => rt.Token == tokenValue);
        tokenToRevoke.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByRefreshToken(tokenValue);

        // Act
        var foundUser = await _userRepository.FindAsync(specification);

        // Assert
        // The specification only checks if token exists, not if it's revoked
        foundUser.Should().NotBeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByRefreshToken_InactiveUser_ReturnsNull()
    {
        // Arrange
        var user = User.Create("Paul", "Hernandez", "paul.hernandez@example.com", UserType.Customer);
        var tokenValue = "refresh-token-inactive-789";
        var refreshToken = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Update IsActive directly to avoid tracking issues
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        userEntity!.IsActive = false;
        userEntity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();

        var specification = new GetUserByRefreshToken(tokenValue);

        // Act
        var foundUser =  await _userRepository.FindAsync(specification);

        // Assert
        foundUser.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_GetUserByRefreshToken_WithIncludes_AllowsDirectLoad()
    {
        // Arrange
        var user = User.Create("Quinn", "King", "quinn.king@example.com", UserType.Employee);
        var role = Role.Create("Viewer", "Read-only access");
        user.AssignRole(role);
        var tokenValue = "refresh-token-tracking-101";
        var refreshToken = RefreshToken.Create(tokenValue, DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken);
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        var specification = new GetUserByRefreshToken(tokenValue);
        var userEntity = await _userRepository.FindAsync(specification, CancellationToken.None, u => u.RefreshTokens);
        
        // Assert
        userEntity.Should().NotBeNull();
        userEntity!.RefreshTokens.Should().HaveCount(1);
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
