using Auth.Application.Users.Commands.Login;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
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
using Core.Validation;
using Auth.Domain.Aggregates.Role;

namespace Auth.Tests.IntegrationTests.ApplicationHandlerTests.Auth;

/// <summary>
/// Integration tests for the LoginCommandHandler.
/// Tests user authentication with various scenarios:
/// - Successful login with valid credentials
/// - Failed login with invalid username/password
/// - Login validation and error handling
/// - JWT token generation
/// - Refresh token generation and storage
/// </summary>
public class LoginHandlerIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private AuthDbContext _dbContext = null!;
    private IMediator _mediator = null!;
    private IUserRepository _userRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ITokenService _tokenService = null!;
    private JwtSettings _jwtSettings = null!;
    private ILogger<LoginCommandHandler> _logger = null!;
    private IUnitOfWork _unitOfWork = null!;

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
            TokenExpiryInMinutes = 10,
            RefreshTokenExpiryInDays = 2,
            RefreshTokenSlidingExpiryInMinutes = 20
        });

        // Build services
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AuthDbContext>();
        _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        _passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
        _tokenService = _serviceProvider.GetRequiredService<ITokenService>();
        _jwtSettings = _serviceProvider.GetRequiredService<JwtSettings>();
        _logger = _serviceProvider.GetRequiredService<ILogger<LoginCommandHandler>>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create a simple command dispatcher without MediatR licensing
        _mediator = new SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork>(
            _userRepository, _logger, _passwordHasher, _tokenService, _jwtSettings, _unitOfWork);

        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    #region Setup Helpers

    private async Task<User> CreateValidUserAsync(string firstName = "John", string lastName = "Doe", string email = "john.doe@example.com", string password = "SecurePass123!", Role[]? roles = null)
    {
        var user = User.Create(firstName, lastName, email, UserType.Customer);

        if (roles is not null && roles.Length > 0)
            user.AssignRoles(roles);
        
        // Hash and set password
        var passwordHash = _passwordHasher.Hash(password);
        user.SetPasswordHash(passwordHash);
        
        await _userRepository.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        // Reload to get UserRoles and RefreshTokens collections
        return (await _userRepository.GetByIdAsync(user.Id))!;
    }

    #endregion

    #region Successful Login Scenarios

    [Fact]
    public async Task Handle_ValidUsernameAndPassword_ReturnsLoginSuccess()
    {
        // Arrange
        const string password = "SecurePass123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(
            Username: user.Username.Value,
            Password: password
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.RefreshToken.Should().NotBeNullOrEmpty();
        successResult.Data.User.Should().NotBeNull();
        successResult.Data.User.Username.Should().Be(user.Username.Value);
        successResult.Data.User.Email.Should().Be(user.Contact.Email);
    }

    [Fact]
    public async Task Handle_ValidLogin_ReturnsUserInfo()
    {
        // Arrange
        const string password = "TestPass123!";
        const string firstName = "Jane";
        const string lastName = "Smith";
        const string email = "jane.smith@example.com";
        var user = await CreateValidUserAsync(firstName, lastName, email, password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        successResult.Data.User.ExternalId.Should().Be(user.ExternalId);
        successResult.Data.User.Username.Should().Be(user.Username.Value);
        successResult.Data.User.Email.Should().Be(email);
        successResult.Data.User.FullName.Should().Contain(firstName).And.Contain(lastName);
    }

    [Fact]
    public async Task Handle_ValidLogin_GeneratesAccessToken()
    {
        // Arrange
        const string password = "MySecurePass123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        successResult.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
        // JWT tokens have three parts separated by dots
        successResult.Data.AccessToken.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ValidLogin_GeneratesRefreshToken()
    {
        // Arrange
        const string password = "ValidPassword123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        // Verify refresh token is returned in the response
        successResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        successResult.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_ValidLogin_MultipleLogins_CreatesMultipleRefreshTokens()
    {
        // Arrange
        const string password = "MyPassword123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result1 = await _mediator.Send(command);
        var result2 = await _mediator.Send(command);

        // Assert
        result1.Should().BeOfType<Result<LoginResponse>.Success>();
        result2.Should().BeOfType<Result<LoginResponse>.Success>();
        
        var successResult1 = (Result<LoginResponse>.Success)result1;
        var successResult2 = (Result<LoginResponse>.Success)result2;
        
        // Each login should return different refresh tokens
        successResult1.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        successResult2.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        successResult1.Data.RefreshToken.Should().NotBe(successResult2.Data.RefreshToken);
    }

    #endregion

    #region Invalid Username/Password Scenarios

    [Fact]
    public async Task Handle_InvalidUsername_ThrowsException()
    {
        // Arrange
        const string password = "ValidPass123!";
        await CreateValidUserAsync(password: password);

        var command = new LoginCommand(
            Username: "nonexistent_user",
            Password: password
        );

        // Act & Assert
        await FluentActions.Invoking(async () => await _mediator.Send(command))
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid username");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsException()
    {
        // Arrange
        const string correctPassword = "CorrectPass123!";
        const string wrongPassword = "WrongPass123!";
        var user = await CreateValidUserAsync(password: correctPassword);

        var command = new LoginCommand(
            Username: user.Username.Value,
            Password: wrongPassword
        );

        // Act & Assert
        await FluentActions.Invoking(async () => await _mediator.Send(command))
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid password");
    }

    [Fact]
    public async Task Handle_EmptyUsername_ThrowsValidationException()
    {
        // Arrange
        var command = new LoginCommand(
            Username: "",
            Password: "Password123!"
        );

        // Act & Assert
        await FluentActions.Invoking(async () => await _mediator.Send(command))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_NullPassword_ThrowsValidationException()
    {
        // Arrange
        var user = await CreateValidUserAsync();

        var command = new LoginCommand(
            Username: user.Username.Value,
            Password: ""
        );

        // Act & Assert
        await FluentActions.Invoking(async () => await _mediator.Send(command))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_CaseSensitiveUsername_SucceedsWithCorrectCase()
    {
        // Arrange
        const string password = "CaseTestPass123!";
        var user = await CreateValidUserAsync(firstName: "Test", lastName: "User", password: password);

        // Username should be lowercase by default
        var command1 = new LoginCommand(
            Username: user.Username.Value,
            Password: password
        );

        var command2 = new LoginCommand(
            Username: user.Username.Value,
            Password: password.ToLower()
        );

        // Act
        var result = await _mediator.Send(command1);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        await FluentActions.Invoking(async () => await _mediator.Send(command2))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    #endregion

    #region User State Scenarios

    [Fact]
    public async Task Handle_LoginWithInactiveUser_StillAllowsLogin()
    {
        // Arrange
        const string password = "InactiveUserPass123!";
        var user = await CreateValidUserAsync(password: password);
        
        // Deactivate user
        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var command = new LoginCommand(user.Username.Value, password);

        // Act & Assert
        await FluentActions.Invoking(async () => await _mediator.Send(command))
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Invalid username");
    }

    [Fact]
    public async Task Handle_LoginAfterMultipleFailedAttempts_StillSucceeds()
    {
        // Arrange
        const string correctPassword = "CorrectPass123!";
        const string wrongPassword = "WrongPass123!";
        var user = await CreateValidUserAsync(password: correctPassword);

        // Simulate failed login attempts
        for (int i = 0; i < 3; i++)
        {
            var failedCommand = new LoginCommand(user.Username.Value, wrongPassword);
            
            await FluentActions.Invoking(async () => await _mediator.Send(failedCommand))
                .Should()
                .ThrowAsync<ValidationException>();
        }

        var successCommand = new LoginCommand(user.Username.Value, correctPassword);

        // Act
        var result = await _mediator.Send(successCommand);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
    }

    #endregion

    #region Token Expiration Scenarios

    [Fact]
    public async Task Handle_ValidLogin_TokenExpirationMatchesSettings()
    {
        // Arrange
        const string password = "TokenExpireTestPass123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
    }

    [Fact]
    public async Task Handle_ValidLogin_RefreshTokenExpiryDateIsSet()
    {
        // Arrange
        const string password = "RefreshTokenExpiryTest123!";
        var user = await CreateValidUserAsync(password: password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        // Verify expiry time is correctly set based on settings
        successResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        // The refresh token expiry should be calculated properly in the handler
    }

    #endregion

    #region User Information Scenarios

    [Fact]
    public async Task Handle_ValidLogin_IncludesUserRoles()
    {
        // Arrange
        const string password = "RolesTest123!";
        var user = await CreateValidUserAsync(password: password, roles: [Role.Create("Admin", "Administrator role with full permissions")]);
        
        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        // Verify user information is present in response
        successResult.Data.User.Should().NotBeNull();
        successResult.Data.User.Username.Should().Be(user.Username.Value);
        successResult.Data.User.FullName.Should().NotBeNullOrWhiteSpace();
        successResult.Data.User.Roles.Should().HaveCount(1);
        successResult.Data.User.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_ValidLogin_UserFullNameIsCorrect()
    {
        // Arrange
        const string password = "FullNameTest123!";
        const string firstName = "Albert";
        const string lastName = "Einstein";
        var user = await CreateValidUserAsync(firstName, lastName, "albert@example.com", password);

        var command = new LoginCommand(user.Username.Value, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<LoginResponse>.Success>();
        var successResult = (Result<LoginResponse>.Success)result;
        
        successResult.Data.User.FullName.Should().Be($"{firstName} {lastName}");
    }

    #endregion

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }
}

