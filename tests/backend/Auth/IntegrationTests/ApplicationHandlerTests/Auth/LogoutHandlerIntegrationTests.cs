using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Auth.Application.Users.Commands.Login;
using Auth.Application.Users.Commands.Logout;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories.Interfaces;
using Common;
using Core.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Tests.IntegrationTests.ApplicationHandlerTests.Auth;

public class LogoutHandlerIntegrationTests : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;
    private AuthDbContext _dbContext = null!;
    private IUserRepository _userRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ITokenService _tokenService = null!;
    private JwtSettings _jwtSettings = null!;
    private ILogger<LoginCommandHandler> _loginLogger = null!;
    private ILogger<LogoutCommandHandler> _logoutLogger = null!;
    private IUnitOfWork _unitOfWork = null!;
    private SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork> _loginMediator = null!;
    private SimpleMediator<IUserRepository, LogoutCommandHandler, LogoutCommand, Result<LogoutResponse>, IUnitOfWork, MediatR.IMediator> _logoutMediator = null!;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        var client = _factory.CreateClient();

        // Resolve services from the factory
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _jwtSettings = scope.ServiceProvider.GetRequiredService<JwtSettings>();
        _loginLogger = scope.ServiceProvider.GetRequiredService<ILogger<LoginCommandHandler>>();
        _logoutLogger = scope.ServiceProvider.GetRequiredService<ILogger<LogoutCommandHandler>>();
        _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Create mediators for login and logout
        _loginMediator = new SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork>(
            _userRepository, _loginLogger, _passwordHasher, _tokenService, _jwtSettings, _unitOfWork);

        var mediatorForHandler = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        _logoutMediator = new SimpleMediator<IUserRepository, LogoutCommandHandler, LogoutCommand, Result<LogoutResponse>, IUnitOfWork, MediatR.IMediator>(
            _userRepository, _logoutLogger, _unitOfWork, mediatorForHandler);
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
    }

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

        // Add user with token to repository
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Create and add refresh token to the user BEFORE persisting
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

    #region Successful Logout Scenarios

    [Fact]
    public async Task Handle_ValidUserAndRefreshToken_SuccessfulLogout()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        var command = new LogoutCommand(
            ExternalId: user.ExternalId,
            RefreshToken: refreshToken);

        // Act
        var result = await _logoutMediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LogoutResponse>.Success>();
        var successResult = (Result<LogoutResponse>.Success)result;

        successResult.Data.Success.Should().BeTrue();
        successResult.Data.Message.Should().Contain("successful");
    }

    [Fact]
    public async Task Handle_ValidLogout_TokenIsRevoked()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        var logoutCommand = new LogoutCommand(user.ExternalId, refreshToken);

        // Act
        var result = await _logoutMediator.Send(logoutCommand);

        // Assert - Verify logout was successful
        result.Should().BeOfType<Result<LogoutResponse>.Success>();

        // Verify token is revoked by checking user's refresh tokens
        // Reload from database to get latest state
        _dbContext.ChangeTracker.Clear();
        var userEntity = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var updatedUser = (User)userEntity!;
        updatedUser!.RefreshTokens.Should().HaveCount(1);
        updatedUser.RefreshTokens.First().IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Logout_RevokesAllActiveRefreshTokens_ForUser()
    {
        // Arrange - create user and two active refresh tokens (multi-device)
        var (user, firstToken) = await CreateUserWithRefreshTokenAsync();

        // Generate a second refresh token by logging in again (second device)
        var loginResult = await _loginMediator.Send(new LoginCommand(user.Username.Value, "SecurePass123!"));
        var secondToken = ((Result<LoginResponse>.Success)loginResult).Data.RefreshToken;

        // Sanity check: both tokens exist in DB
        _dbContext.ChangeTracker.Clear();
        var userEntityBefore = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);
        var savedUserBefore = (User)userEntityBefore!;
        savedUserBefore.RefreshTokens.Select(t => t.Token).Should().Contain(new[] { firstToken, secondToken });

        // Act - logout using the first token
        var logoutCommand = new LogoutCommand(user.ExternalId, firstToken);
        var result = await _logoutMediator.Send(logoutCommand);

        // Assert - both tokens must be revoked for that user
        result.Should().BeOfType<Result<LogoutResponse>.Success>();

        _dbContext.ChangeTracker.Clear();
        var userEntityAfter = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var updatedUser = (User)userEntityAfter!;
        updatedUser.RefreshTokens.Should().OnlyContain(t => t.IsRevoked);
    }
    [Fact]
    public async Task Handle_MultipleLogoutsWithSameToken_ThrowsException()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        // Act - Logout first token
        var logoutCommand = new LogoutCommand(user.ExternalId, refreshToken);
        var logoutResult1 = await _logoutMediator.Send(logoutCommand);

        // Assert first logout
        logoutResult1.Should().BeOfType<Result<LogoutResponse>.Success>();

        // Act & Assert - Logout second token
        await FluentActions.Invoking(async () => await _logoutMediator.Send(logoutCommand))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token is already revoked");

        // Verify both tokens are revoked
        _dbContext.ChangeTracker.Clear();
        var userEntity = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value);

        var updatedUser = (User)userEntity!;
        updatedUser!.RefreshTokens.Should().HaveCount(1);
        updatedUser.RefreshTokens.First().IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidLogout_ReturnsSuccessMessage()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        var command = new LogoutCommand(user.ExternalId, refreshToken);

        // Act
        var result = await _logoutMediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LogoutResponse>.Success>();
        var successResult = (Result<LogoutResponse>.Success)result;

        successResult.Data.Should().NotBeNull();
        successResult.Data.Message.Should().NotBeNullOrWhiteSpace();
        successResult.Data.Message.Should().Contain("revoked");
    }

    #endregion

    #region Invalid User Scenarios

    [Fact]
    public async Task Handle_NonexistentUser_ThrowsException()
    {
        // Arrange
        var command = new LogoutCommand(
            ExternalId: 999999L,
            RefreshToken: "some-token");

        // Act & Assert
        var response = await _logoutMediator.Send(command);

        response.Should().NotBeNull();
        response.Should().BeOfType<Result<LogoutResponse>.ErrorState>();
        var errorResponse = (Result<LogoutResponse>.ErrorState)response;
        errorResponse.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsException()
    {
        // Arrange - Use an invalid ExternalId
        var command = new LogoutCommand(
            ExternalId: 0L,
            RefreshToken: "some-token");

        // Act & Assert
        var response = await _logoutMediator.Send(command);

        response.Should().NotBeNull();
        response.Should().BeOfType<Result<LogoutResponse>.ErrorState>();
        var errorResponse = (Result<LogoutResponse>.ErrorState)response;
        errorResponse.Message.Should().Be("User not found");
    }

    #endregion

    #region Invalid Token Scenarios

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsException()
    {
        // Arrange
        var (user, _) = await CreateUserWithRefreshTokenAsync();

        var command = new LogoutCommand(
            ExternalId: user.ExternalId,
            RefreshToken: "invalid-token");

        // Act & Assert
        await FluentActions.Invoking(async () => await _logoutMediator.Send(command))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_EmptyRefreshToken_ThrowsException()
    {
        // Arrange
        var (user, _) = await CreateUserWithRefreshTokenAsync();

        var command = new LogoutCommand(
            ExternalId: user.ExternalId,
            RefreshToken: string.Empty);

        // Act & Assert
        await FluentActions.Invoking(async () => await _logoutMediator.Send(command))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_ThrowsException()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        // First logout
        var firstLogoutCommand = new LogoutCommand(user.ExternalId, refreshToken);
        await _logoutMediator.Send(firstLogoutCommand);

        // Try to logout again with same token
        var secondLogoutCommand = new LogoutCommand(user.ExternalId, refreshToken);

        // Act & Assert
        await FluentActions.Invoking(async () => await _logoutMediator.Send(secondLogoutCommand))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_LogoutDoesNotAffectOtherUsers()
    {
        // Arrange
        var (user1, token1) = await CreateUserWithRefreshTokenAsync("User", "One", "user1@example.com");
        var (user2, token2) = await CreateUserWithRefreshTokenAsync("User", "Two", "user2@example.com");

        // Act - Logout user1
        var logoutCommand = new LogoutCommand(user1.ExternalId, token1);
        await _logoutMediator.Send(logoutCommand);

        // Assert - User1 has no tokens
        _dbContext.ChangeTracker.Clear();
        var userEntity1 = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user1.Id.Value);
        var updatedUser1 = (User)userEntity1!;
        updatedUser1!.RefreshTokens.Should().HaveCount(1);
        updatedUser1.RefreshTokens.First().IsRevoked.Should().BeTrue();

        // Assert - User2 still has tokens
        _dbContext.ChangeTracker.Clear();
        var userEntity2 = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user2.Id.Value);
        var updatedUser2 = (User)userEntity2!;
        updatedUser2!.RefreshTokens.Should().NotBeEmpty();
        updatedUser2.RefreshTokens.First().Token.Should().Be(token2);
    }

    [Fact]
    public async Task Handle_LogoutWithCorrectTokenFormat_Succeeds()
    {
        // Arrange
        var (user, refreshToken) = await CreateUserWithRefreshTokenAsync();

        var command = new LogoutCommand(user.ExternalId, refreshToken);

        // Act
        var result = await _logoutMediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LogoutResponse>.Success>();
        refreshToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Length.Should().BeGreaterThan(10); // JWT-like token
    }

    #endregion
}
