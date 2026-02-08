using System.Net;
using System.Net.Http.Json;
using Auth.Application.Users.Commands.Login;
using Auth.Infrastructure.Repositories.Interfaces;
using Common;
using Auth.Infrastructure.Specifications;
using Core.API;
using Microsoft.AspNetCore.Http;

namespace Auth.Tests.IntegrationTests.ApiTests.Auth;

/// <summary>
/// Integration tests for the Login endpoint.
/// Tests the complete flow: HTTP request → API → Handler → Domain → Database.
/// </summary>
public class LoginEndpointTests(TestWebApplicationFactory factory) : BaseAuthApiTests(factory)
{
    #region Success Scenarios

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkStatus()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("testuser", "Test@123!");

        var request = new LoginCommand(
            Username: "testuser",
            Password: "Test@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsLoginResponseWithTokens()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("validuser", "Valid@Pass123!");

        var request = new LoginCommand(
            Username: "validuser",
            Password: "Valid@Pass123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeEmpty();
        loginResponse.RefreshToken.Should().NotBeEmpty();
        loginResponse.ExpiresInSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithValidCredentials_AccessTokenIsValidJwt()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("jwtuser", "Jwt@Pass123!");

        var request = new LoginCommand(
            Username: "jwtuser",
            Password: "Jwt@Pass123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        // JWT tokens have 3 parts separated by dots
        var tokenParts = loginResponse!.AccessToken.Split('.');
        tokenParts.Length.Should().Be(3);
        tokenParts.All(part => !string.IsNullOrEmpty(part)).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithValidCredentials_RefreshTokenIsStored()
    {
        // Arrange
        var client = CreateHttpClient();
        var username = "storetokenuser";
        await CreateTestUserAsync(username, "Storage@123!");

        var request = new LoginCommand(
            Username: username,
            Password: "Storage@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert - Verify refresh token is persisted in database
        loginResponse.Should().NotBeNull();
        loginResponse!.RefreshToken.Should().NotBeEmpty();
        // Token should be retrievable from database
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(loginResponse.RefreshToken), CancellationToken.None);
        user.Should().NotBeNull();
        user!.RefreshTokens.Should().Contain(t => t.Token == loginResponse.RefreshToken);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsCorrectExpirationTime()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("expiryuser", "Expiry@123!");

        var request = new LoginCommand(
            Username: "expiryuser",
            Password: "Expiry@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        // Token should expire in approximately 15 minutes (900 seconds)
        loginResponse!.ExpiresInSeconds.Should().BeInRange(800, 1000);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUserInformation()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("infouser", "Info@Pass123!");

        var request = new LoginCommand(
            Username: "infouser",
            Password: "Info@Pass123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        loginResponse!.User.Should().NotBeNull();
        loginResponse.User.Username.Should().Be("infouser");
    }

    [Fact]
    public async Task Login_MultipleTimes_GeneratesDifferentTokens()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("multiuser", "Multi@Pass123!");

        var request = new LoginCommand(
            Username: "multiuser",
            Password: "Multi@Pass123!"
        );

        // Act
        var response1 = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse1 = await response1.Content.ReadFromJsonAsync<LoginResponse>();

        await Task.Delay(1000); // Ensure time difference for token generation

        var response2 = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse2 = await response2.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse1.Should().NotBeNull();
        loginResponse2.Should().NotBeNull();
        loginResponse1!.AccessToken.Should().NotBe(loginResponse2!.AccessToken);
        loginResponse1.RefreshToken.Should().NotBe(loginResponse2.RefreshToken);
    }

    [Fact]
    public async Task Login_MultipleUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("user1", "Multi@Pass123!");
        await CreateTestUserAsync("user2", "Multi@Pass123!");

        var request = new LoginCommand(
            Username: "user1",
            Password: "Multi@Pass123!"
        );

        var request2 = new LoginCommand(
            Username: "user2",
            Password: "Multi@Pass123!"
        );

        // Act
        var response1 = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse1 = await response1.Content.ReadFromJsonAsync<LoginResponse>();

        var response2 = await client.PostAsJsonAsync("/api/auth/login", request2);
        var loginResponse2 = await response2.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse1.Should().NotBeNull();
        loginResponse2.Should().NotBeNull();
        loginResponse1!.AccessToken.Should().NotBe(loginResponse2!.AccessToken);
        loginResponse1.RefreshToken.Should().NotBe(loginResponse2.RefreshToken);
    }

    #endregion

    #region Invalid Credentials Scenarios

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("wrongpwd", "Correct@Pass123!");

        var request = new LoginCommand(
            Username: "wrongpwd",
            Password: "Wrong@Pass123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid password");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithNonexistentUsername_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();

        var request = new LoginCommand(
            Username: "nonexistent",
            Password: "SomePassword@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid username");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();

        var request = new LoginCommand(
            Username: "",
            Password: "SomePassword@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Contain("Invalid username");
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("testuser", "Correct@Pass123!");

        var request = new LoginCommand(
            Username: "testuser",
            Password: ""
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid password");
    }

    [Fact]
    public async Task Login_WithNullUsername_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();

        var request = new LoginCommand(
            Username: null!,
            Password: "SomePassword@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Contain("Invalid username");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithNullPassword_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("testuser", "Correct@Pass123!");

        var request = new LoginCommand(
            Username: "testuser",
            Password: null!
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid password");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithWrongCaseUsername_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("casetest", "Case@Test123!");

        var request = new LoginCommand(
            Username: "CaseTest", // Different case
            Password: "Case@Test123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid username");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithWhitespaceInPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("spacepwd", "Correct@Pass123!");

        var request = new LoginCommand(
            Username: "spacepwd",
            Password: "Correct@Pass123!  " // Extra spaces
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid password");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Login_WithWhitespaceInUsername_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("username", "Pass@123!");

        var request = new LoginCommand(
            Username: "  username  ", // Extra spaces
            Password: "Pass@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Invalid username");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task Login_ResponseIncludesAllFields()
    {
        // Arrange
        var client = CreateHttpClient();
        await CreateTestUserAsync("nameuser", "Name@123!", "name@test.com", "John Doe");

        var request = new LoginCommand(
            Username: "nameuser",
            Password: "Name@123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNull();
        loginResponse.RefreshToken.Should().NotBeNull();
        loginResponse.ExpiresInSeconds.Should().BeGreaterThan(0);
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Id.Should().NotBeEmpty();
        loginResponse.User.Email.Should().NotBeNull();
        loginResponse.User.FullName.Should().Be("John Doe");
    }

    #endregion
}
