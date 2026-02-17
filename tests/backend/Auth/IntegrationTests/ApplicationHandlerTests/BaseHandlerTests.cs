using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Auth.Application.Users.Commands.Login;
using Auth.Application.Users.Commands.RefreshToken;
using Auth.Domain;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Repositories.Interfaces;
using Common;
using Core.API;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Tests.IntegrationTests.ApplicationHandlerTests;

public abstract class BaseHandlerTests : IAsyncLifetime
{
    protected User UserTest { get; private set; }
    protected readonly string TestUserPassword = "Test@123!";
    protected readonly ServiceProvider _serviceProvider;
    protected readonly AuthDbContext _dbContext;
    protected readonly IUserRepository _userRepository;
    protected readonly IPasswordHasher _passwordHasher;
    protected readonly ITokenService _tokenService;
    protected readonly JwtSettings _jwtSettings;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMediator _mediator;
    protected readonly SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork> _loginMediator;


    public BaseHandlerTests()
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
        services.AddScoped(_ => new JwtSettings
        {
            SecretKey = "this-is-a-very-long-secret-key-for-jwt-testing-purposes-minimum-32-characters",
            Issuer = "LibraryApp",
            Audience = "LibraryAppUsers",
            TokenExpiryInMinutes = 10,
            RefreshTokenExpiryInDays = 2,
            RefreshTokenSlidingExpiryInMinutes = 20
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

        // Create a simple mediator for login command to generate refresh tokens for testing
        var loginLogger = _serviceProvider.GetRequiredService<ILogger<LoginCommandHandler>>();
        _loginMediator = new SimpleMediator<IUserRepository, LoginCommandHandler, LoginCommand, Result<LoginResponse>, IPasswordHasher, ITokenService, JwtSettings, IUnitOfWork>(
            _userRepository, loginLogger, _passwordHasher, _tokenService, _jwtSettings, _unitOfWork);

        UserTest = User.Create(
                firstName: "Test",
                lastName: "User",
                email: $"test.{Guid.NewGuid()}@example.com",
                userType: UserType.Employee,
                username: null,
                phoneNumber: null);

        UserTest.PasswordHash = _passwordHasher.Hash(TestUserPassword);
    }

    public virtual async Task InitializeAsync()
    {
        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider == null) return;

        try
        {
            // Delete test users by username pattern to prevent state leakage between tests
            var testUsers = await _dbContext.Users.ToListAsync();

            if (testUsers.Count != 0)
            {
                foreach (var user in testUsers)
                {
                    _dbContext.Users.Remove(user);
                }
                await _dbContext.SaveChangesAsync();
            }

            // Clear the change tracker
            _dbContext.ChangeTracker.Clear();
        }
        finally
        {
            _serviceProvider?.Dispose();
        }
    }

    #region Helper Methods

    /// <summary>
    /// Creates a user with a valid refresh token for testing
    /// </summary>
    protected async Task<LoginResponse> CreateUserWithRefreshTokenAsync(bool rememberMe = false)
    {
        try
        {
            // Add user to repository
            await _userRepository.AddAsync(UserTest);
            await _dbContext.SaveChangesAsync();
            
            // Ensure transaction is fully committed
            await Task.Delay(50);

            // Login to generate refresh token
            var loginCommand = new LoginCommand(UserTest.Username.Value, TestUserPassword, rememberMe);
            var loginResult = await _loginMediator.Send(loginCommand);
            var successResult = (Result<LoginResponse>.Success)loginResult;

            // Ensure the login transaction is committed
            await Task.Delay(50);
            
            // Clear tracker first, then reload with fresh context
            _dbContext.ChangeTracker.Clear();

            // Reload user from database to ensure it's properly persisted with tokens
            // Create a new DbContext scope to guarantee fresh data
            var savedUser = await _dbContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);
            
            UserTest = savedUser ?? throw new InvalidOperationException("Failed to create test user with refresh token");

            return successResult.Data;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating test user with refresh token", ex);
        }
    }

    #endregion
}
