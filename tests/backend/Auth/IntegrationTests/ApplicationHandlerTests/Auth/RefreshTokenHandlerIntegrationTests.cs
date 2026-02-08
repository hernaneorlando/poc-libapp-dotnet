using Auth.Application.Users.Commands.RefreshToken;
using Auth.Application.Users.Commands.Login;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Repositories.Interfaces;
using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Common;
using Core.API;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Tests.IntegrationTests.ApplicationHandlerTests.Auth;

/// <summary>
/// Integration tests for the RefreshTokenCommandHandler.
/// Tests token refresh functionality with various scenarios:
/// - Successful token refresh with valid refresh token
/// - Failed refresh with invalid/expired/revoked refresh token
/// - New refresh token is generated and stored
/// - Old refresh token is revoked after successful refresh
/// - Multiple refresh tokens handling
/// </summary>
public class RefreshTokenHandlerIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private AuthDbContext _dbContext = null!;
    private IUserRepository _userRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ITokenService _tokenService = null!;
    private JwtSettings _jwtSettings = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IMediator _mediator = null!;
    private SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork> _loginMediator = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Configure AuthDbContext with in-memory database
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add authentication services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        
        // Add JWT settings
        services.AddScoped<JwtSettings>(_ => new JwtSettings
        {
            SecretKey = "this-is-a-very-long-secret-key-for-jwt-testing-purposes-minimum-32-characters",
            Issuer = "LibraryApp",
            Audience = "LibraryAppUsers",
            TokenExpiryInMinutes = 15,
            RefreshTokenExpiryInDays = 7
        });

        // Add MediatR for handling commands
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RefreshTokenCommandHandler).Assembly));

        // Build services
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AuthDbContext>();
        _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        _passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
        _tokenService = _serviceProvider.GetRequiredService<ITokenService>();
        _jwtSettings = _serviceProvider.GetRequiredService<JwtSettings>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        var loginLogger = _serviceProvider.GetRequiredService<ILogger<LoginCommandHandler>>();
        _loginMediator = new SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork>(
            _userRepository, loginLogger, _passwordHasher, _tokenService, _jwtSettings, _unitOfWork);

        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a user with a valid refresh token for testing
    /// </summary>
    private async Task<(User User, string RefreshToken)> CreateUserWithRefreshTokenAsync(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string password = "SecurePass123!")
    {
        // Create user
        var user = User.Create(firstName, lastName, email, UserType.Customer);
        var passwordHash = _passwordHasher.Hash(password);
        user.SetPasswordHash(passwordHash);

        // Add user to repository
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Login to generate refresh token
        var loginCommand = new LoginCommand(user.Username.Value, password);
        var loginResult = await _loginMediator.Send(loginCommand);
        var successResult = (Result<LoginResponse>.Success)loginResult;

        // Detach all tracked entities to avoid conflicts
        _dbContext.ChangeTracker.Clear();

        // Reload user from database to ensure it's properly persisted with tokens
        var savedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        return (savedUser!, successResult.Data.RefreshToken);
    }

    #endregion

    #region Successful Refresh Scenarios

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var (_, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        
        var command = new RefreshTokenCommand(oldRefreshToken);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RefreshTokenResponse>.Success>();
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.RefreshToken.Should().NotBeNullOrEmpty();
        successResult.Data.ExpiresInSeconds.Should().Be(_jwtSettings.TokenExpiryInMinutes * 60);
        successResult.Data.RefreshToken.Should().NotBe(oldRefreshToken);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_OldTokenIsRevoked()
    {
        // Arrange
        var (user, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        
        var command = new RefreshTokenCommand(oldRefreshToken);

        // Act
        await _mediator.Send(command);

        // Assert - Verify old token is revoked
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var oldToken = updatedUser!.RefreshTokens
            .FirstOrDefault(rt => rt.Token == oldRefreshToken);
        
        oldToken.Should().NotBeNull();
        oldToken!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_NewTokenIsAdded()
    {
        // Arrange
        var (user, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        
        var command = new RefreshTokenCommand(oldRefreshToken);

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        var newRefreshToken = successResult.Data.RefreshToken;

        // Assert - Verify new token is in database
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var newToken = updatedUser!.RefreshTokens
            .FirstOrDefault(rt => rt.Token == newRefreshToken);
        
        newToken.Should().NotBeNull();
        newToken!.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_TokenCountIncreases()
    {
        // Arrange
        var (user, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        
        _dbContext.ChangeTracker.Clear();
        var userBefore = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        var tokenCountBefore = userBefore!.RefreshTokens.Count;

        var command = new RefreshTokenCommand(oldRefreshToken);

        // Act
        await _mediator.Send(command);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var userAfter = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        userAfter!.RefreshTokens.Count.Should().Be(tokenCountBefore + 1);
    }

    #endregion

    #region Invalid Refresh Token Scenarios

    [Fact]
    public async Task Handle_RefreshTokenNotFound_ThrowsException()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token-that-does-not-exist";
        var command = new RefreshTokenCommand(invalidToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _mediator.Send(command));
    }

    [Fact]
    public async Task Handle_EmptyRefreshToken_ThrowsException()
    {
        // Arrange
        var command = new RefreshTokenCommand(string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _mediator.Send(command));
    }

    [Fact]
    public async Task Handle_RevokedRefreshToken_ThrowsException()
    {
        // Arrange
        var (user, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        
        // First refresh to revoke the old token
        var firstRefreshCommand = new RefreshTokenCommand(oldRefreshToken);
        await _mediator.Send(firstRefreshCommand);

        // Try to refresh with the now-revoked token
        var secondRefreshCommand = new RefreshTokenCommand(oldRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _mediator.Send(secondRefreshCommand));
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ThrowsException()
    {
        // Arrange - Create user with expired token
        var user = User.Create("John", "Doe", "john.doe@example.com", UserType.Customer);
        var passwordHash = _passwordHasher.Hash("SecurePass123!");
        user.SetPasswordHash(passwordHash);

        // Create expired refresh token (expires in the past)
        var expiredTokenString = _tokenService.GenerateRefreshToken(user.Id.Value);
        var expiredToken = RefreshToken.Create(expiredTokenString, DateTime.UtcNow.AddDays(1));
        expiredToken.ExpiresAt = DateTime.UtcNow.AddDays(-1); // Set expiry to past
        user.AddRefreshToken(expiredToken);

        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand(expiredTokenString);

        // Act & Assert
        await FluentActions.Awaiting(async () => await _mediator.Send(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token is invalid, expired, or revoked");
    }

    #endregion

    #region Multiple Token Scenarios

    [Fact]
    public async Task Handle_MultipleValidTokens_RefreshesOnlySpecifiedToken()
    {
        // Arrange - Create user with first token
        var (user, token1) = await CreateUserWithRefreshTokenAsync();

        // Add second token to domain model
        var domainUser = await _userRepository.GetByIdAsync(user.Id);
        var token2String = _tokenService.GenerateRefreshToken(user.Id.Value);
        var token2 = RefreshToken.Create(token2String, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays));
        domainUser!.AddRefreshToken(token2);
        await _userRepository.UpdateAsync(domainUser);
        await _dbContext.SaveChangesAsync();

        // Refresh with first token
        var command = new RefreshTokenCommand(token1);

        // Act
        await _mediator.Send(command);

        // Assert - First token should be revoked, second token should still be valid
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var revokedToken = updatedUser!.RefreshTokens.FirstOrDefault(rt => rt.Token == token1);
        var validToken = updatedUser.RefreshTokens.FirstOrDefault(rt => rt.Token == token2String);

        revokedToken.Should().NotBeNull();
        revokedToken!.RevokedAt.Should().NotBeNull();
        validToken.Should().NotBeNull();
        validToken!.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MultipleRefreshes_EachTokenBecomesRevoked()
    {
        // Arrange
        var (user, token1) = await CreateUserWithRefreshTokenAsync();

        // First refresh
        var refreshCommand1 = new RefreshTokenCommand(token1);
        var result1 = await _mediator.Send(refreshCommand1);
        var successResult1 = (Result<RefreshTokenResponse>.Success)result1;
        var token2 = successResult1.Data.RefreshToken;

        // Second refresh
        var refreshCommand2 = new RefreshTokenCommand(token2);
        var result2 = await _mediator.Send(refreshCommand2);
        var successResult2 = (Result<RefreshTokenResponse>.Success)result2;
        var token3 = successResult2.Data.RefreshToken;

        // Assert - Both token1 and token2 should be revoked
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var firstToken = updatedUser!.RefreshTokens.FirstOrDefault(rt => rt.Token == token1);
        var secondToken = updatedUser.RefreshTokens.FirstOrDefault(rt => rt.Token == token2);
        var thirdToken = updatedUser.RefreshTokens.FirstOrDefault(rt => rt.Token == token3);

        firstToken!.RevokedAt.Should().NotBeNull("First token should be revoked");
        secondToken!.RevokedAt.Should().NotBeNull("Second token should be revoked");
        thirdToken!.RevokedAt.Should().BeNull("Third token should not be revoked");
    }

    #endregion

    #region Token Validity Scenarios

    [Fact]
    public async Task Handle_NewAccessTokenIsValid()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();
        var command = new RefreshTokenCommand(refreshToken);

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;

        // Assert - Access token should be non-empty (format validation would require JWT parsing)
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.AccessToken.Should().Contain(".");  // JWT format check
    }

    [Fact]
    public async Task Handle_NewRefreshTokenHasCorrectExpiry()
    {
        // Arrange
        var (user, oldRefreshToken) = await CreateUserWithRefreshTokenAsync();
        var command = new RefreshTokenCommand(oldRefreshToken);

        var timeBeforeRefresh = DateTime.UtcNow;

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        var newRefreshToken = successResult.Data.RefreshToken;

        var timeAfterRefresh = DateTime.UtcNow;

        // Assert - New token should exist in database with correct expiry
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var token = updatedUser!.RefreshTokens.FirstOrDefault(rt => rt.Token == newRefreshToken);
        token.Should().NotBeNull();

        var expectedExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
        token!.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    #endregion
}
