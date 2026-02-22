using System.Net.Http.Headers;
using System.Net.Http.Json;
using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Auth.Application.Users.Commands.Login;
using Auth.Domain;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories.Interfaces;
using Common;
using Core.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Tests.IntegrationTests.ApiTests;

/// <summary>
/// Base class for Auth context API integration tests.
/// Configures the WebApplicationFactory with in-memory databases and provides utility methods.
/// Shared by all Auth-related API tests (Roles, Users, Authentication, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the BaseApiTests class.
/// Configures the test environment with in-memory SQL and NoSQL databases.
/// </remarks>
public abstract class BaseApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory _webFactory;
    protected readonly User UserTest;
    protected readonly string TestUserPassword = "Test@123!";

    public BaseApiTests(TestWebApplicationFactory factory)
    {
        _webFactory = factory;
        
        UserTest = User.Create(
                firstName: "Test",
                lastName: "User",
                email: $"test.{Guid.NewGuid()}@example.com",
                userType: UserType.Employee,
                username: null,
                phoneNumber: null);

        UserTest.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestUserPassword, workFactor: PasswordHasher.WorkFactor);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_webFactory?.Services == null) return;

        try
        {
            using var scope = _webFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            // Delete test users by username pattern to prevent state leakage between tests
            var testUsers = await dbContext.Users.ToListAsync();

            if (testUsers.Count != 0)
            {
                foreach (var user in testUsers)
                {
                    dbContext.Users.Remove(user);
                }
                await dbContext.SaveChangesAsync();
            }

            // Clear the change tracker
            dbContext.ChangeTracker.Clear();
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    /// <summary>
    /// Creates a new HttpClient configured for the test.
    /// </summary>
    protected HttpClient CreateHttpClient()
    {
        return _webFactory.CreateClient();
    }

    /// <summary>
    /// Creates a new HttpClient, with Bearer token, configured for the test.
    /// </summary>
    protected HttpClient CreateHttpClient(string accessToken)
    {
        var client = _webFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected TService GetService<TService>() where TService : notnull
    {
        if (_webFactory?.Services == null)
            throw new InvalidOperationException("WebApplicationFactory has been disposed!");

        using var scope = _webFactory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    /// <summary>
    /// Creates a test user in the in-memory database.
    /// </summary>
    /// <param name="username">Username of the test user.</param>
    /// <param name="password">Password for the test user.</param>
    /// <param name="email">Email address of the test user.</param>
    /// <param name="fullName">Full name of the test user.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task CreateTestUserAsync(
    string username,
    string password,
    string? email = null,
    string? fullName = null)
    {
        email ??= $"{username}@test.com";
        fullName ??= "Test User";
        var firstAndLastName = fullName.Split(' ');
        var firstName = firstAndLastName[0];
        var lastName = firstAndLastName.Length > 1 ? firstAndLastName[^1] : "User";

        if (_webFactory?.Services == null)
            throw new InvalidOperationException("WebApplicationFactory has been disposed!");

        using var scope = _webFactory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Create user with Username value object
        var usernameValueObject = Username.Create(username);
        var user = User.Create(firstName, lastName, email, UserType.Customer, username: usernameValueObject);
        var passwordHash = passwordHasher.Hash(password);
        user.SetPasswordHash(passwordHash);
        user.ExternalId = 1L; // Set a fixed ExternalId for testing purposes

        // Persist user
        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a test user and logs them in, returning userId, access token and refresh token.
    /// </summary>
    protected async Task<LoginResponse> LoginTestUserAsync(string username, string password, bool createNewUser = true)
    {
        // Create user
        if (createNewUser)
            await CreateTestUserAsync(username, password);

        // Login to get tokens
        var client = CreateHttpClient();
        var loginRequest = new LoginCommand(
            Username: username,
            Password: password
        );

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();

        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue();
        loginResult.Value.Should().NotBeNull();
        return loginResult!.Value!;
    }

    /// <summary>
    /// Creates an authenticated HttpClient with an authorized user that has the specified permissions.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedHttpClientWithPermissions(
        params (string Feature, string Action)[] permissions)
    {
        // Create client FIRST (outside the scope) so it doesn't depend on disposed services
        var client = CreateHttpClient();

        // Use a scope ONLY for creating the user and token
        if (_webFactory?.Services == null)
        {
            throw new InvalidOperationException("WebApplicationFactory has been disposed!");
        }

        using (var scope = _webFactory.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Create a role with the specified permissions
            var role = Role.Create($"Test Role {Guid.NewGuid()}", "Role for testing API endpoints");
            foreach (var (feature, action) in permissions)
            {
                var featureEnum = Enum.Parse<PermissionFeature>(feature);
                var actionEnum = Enum.Parse<PermissionAction>(action);
                role.AssignPermission(new Permission(featureEnum, actionEnum));
            }

            // Create a user with the role
            UserTest.AssignRole(role);
            await userRepository.AddAsync(UserTest);

            // Persist changes with explicit save
            await unitOfWork.SaveChangesAsync();

            // Verify user was actually saved
            if (await userRepository.GetByIdAsync(UserTest.Id) is null)
            {
                throw new InvalidOperationException($"User with ID {UserTest.Id} was not saved to the database!");
            }

            // Generate JWT token for the user
            var token = tokenService.GenerateAccessToken(UserTest);

            // Add authentication header to the client
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        // Scope is disposed here, but client is still valid and has the Bearer token set

        return client;
    }

    /// <summary>
    /// Creates an authenticated HttpClient with a user that has the Role.Create permission specifically.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedHttpClientForRoleCreation()
    {
        return await CreateAuthenticatedHttpClientWithPermissions(
            (PermissionFeature.Role.ToString(), PermissionAction.Create.ToString())
        );
    }

    /// <summary>
    /// Creates an authenticated HttpClient for logout operations.
    /// The logout endpoint only requires authentication, not specific permissions.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedHttpClientForLogout()
    {
        return await CreateAuthenticatedHttpClientWithPermissions(
            (PermissionFeature.User.ToString(), PermissionAction.Read.ToString())
        );
    }

    /// <summary>
    /// Creates a user without the required permissions.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedHttpClientWithoutRequiredPermissions()
    {
        return await CreateAuthenticatedHttpClientWithPermissions(
            (PermissionFeature.User.ToString(), PermissionAction.Read.ToString())
        );
    }

    /// <summary>
    /// Creates an authenticated HttpClient for role read operations (list roles).
    /// Includes Role.Read permission.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedHttpClientForRoleRead()
    {
        return await CreateAuthenticatedHttpClientWithPermissions(
            (PermissionFeature.Role.ToString(), PermissionAction.Read.ToString())
        );
    }


}
