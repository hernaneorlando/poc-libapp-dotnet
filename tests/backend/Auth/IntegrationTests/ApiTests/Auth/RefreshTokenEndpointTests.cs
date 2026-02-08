using System.Net;
using System.Net.Http.Json;
using Auth.Application.Users.Commands.RefreshToken;
using Auth.Application.Users.Commands.Logout;
using Auth.Domain.ValueObjects;
using Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Common;
using Core.API;
using Microsoft.AspNetCore.Http;
using Auth.Infrastructure.Specifications;
using Auth.Domain;

namespace Auth.Tests.IntegrationTests.ApiTests.Auth;

/// <summary>
/// Integration tests for the RefreshToken endpoint.
/// Tests the complete flow: HTTP request → API → Handler → Domain → Database.
/// </summary>
public class RefreshTokenEndpointTests(TestWebApplicationFactory factory) : BaseAuthApiTests(factory)
{
    #region Success Scenarios

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsOkStatus()
    {
        // Arrange
        var login = await LoginTestUserAsync("refreshuser", "Refresh@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsRefreshTokenResponse()
    {
        // Arrange
        var login = await LoginTestUserAsync("validrefresh", "Valid@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.AccessToken.Should().NotBeEmpty();
        refreshResponse.RefreshToken.Should().NotBeEmpty();
        refreshResponse.ExpiresInSeconds.Should().BeGreaterThan(0);
        refreshResponse.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_GeneratesDifferentAccessToken()
    {
        // Arrange
        var login = await LoginTestUserAsync("differentaccess", "Different@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);
        var oldAccessToken = login.AccessToken;

        // Add delay to ensure different token generation (different timestamp)
        await Task.Delay(1100);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.AccessToken.Should().NotBe(oldAccessToken);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_GeneratesDifferentRefreshToken()
    {
        // Arrange
        var login = await LoginTestUserAsync("differentrefresh", "Different@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);
        var oldRefreshToken = login.RefreshToken;

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.RefreshToken.Should().NotBe(oldRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_AccessTokenIsValidJwt()
    {
        // Arrange
        var login = await LoginTestUserAsync("jwtrefresh", "Jwt@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        var tokenParts = refreshResponse!.AccessToken.Split('.');
        tokenParts.Length.Should().Be(3);
        tokenParts.All(part => !string.IsNullOrEmpty(part)).Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_RefreshTokenIsStored()
    {
        // Arrange
        var login = await LoginTestUserAsync("storenewtoken", "Store@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert - Verify new refresh token is persisted in database
        refreshResponse.Should().NotBeNull();
        refreshResponse!.RefreshToken.Should().NotBeEmpty();
        
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(refreshResponse.RefreshToken), CancellationToken.None);
        user.Should().NotBeNull();
        user!.RefreshTokens.Should().Contain(t => t.Token == refreshResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsCorrectExpirationTime()
    {
        // Arrange
        var login = await LoginTestUserAsync("expiryrefresh", "Expiry@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        // Token should expire in approximately 15 minutes (900 seconds)
        refreshResponse!.ExpiresInSeconds.Should().BeInRange(800, 1000);
    }

    [Fact]
    public async Task RefreshToken_MultipleTimes_GeneratesDifferentTokensEachTime()
    {
        // Arrange
        var login = await LoginTestUserAsync("multirefresh", "Multi@123!");
        var client = CreateHttpClient();

        var tokens = new List<RefreshTokenResponse>();

        // Act - Refresh multiple times
        for (int i = 0; i < 3; i++)
        {
            var currentRefreshToken = i == 0 ? login.RefreshToken : tokens[i - 1].RefreshToken;
            var request = new RefreshTokenCommand(RefreshToken: currentRefreshToken);
            var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
            var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

            tokens.Add(refreshResponse!);
            
            // Delay to ensure different token generation times (JWT includes 'iat' claim in seconds)
            if (i < 2)
                await Task.Delay(1100);
        }

        // Assert - All tokens should be different
        for (int i = 0; i < tokens.Count; i++)
        {
            for (int j = i + 1; j < tokens.Count; j++)
            {
                tokens[i].AccessToken.Should().NotBe(tokens[j].AccessToken);
                tokens[i].RefreshToken.Should().NotBe(tokens[j].RefreshToken);
            }
        }
    }

    [Fact]
    public async Task RefreshToken_DifferentUsers_GenerateDifferentTokens()
    {
        // Arrange
        var login1 = await LoginTestUserAsync("user1refresh", "User1@123!");
        var login2 = await LoginTestUserAsync("user2refresh", "User2@123!");
        var client = CreateHttpClient();

        var request1 = new RefreshTokenCommand(RefreshToken: login1.RefreshToken);
        var request2 = new RefreshTokenCommand(RefreshToken: login2.RefreshToken);

        // Act
        var response1 = await client.PostAsJsonAsync("/api/auth/refresh", request1);
        var refreshResponse1 = await response1.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        var response2 = await client.PostAsJsonAsync("/api/auth/refresh", request2);
        var refreshResponse2 = await response2.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse1.Should().NotBeNull();
        refreshResponse2.Should().NotBeNull();
        refreshResponse1!.AccessToken.Should().NotBe(refreshResponse2!.AccessToken);
        refreshResponse1.RefreshToken.Should().NotBe(refreshResponse2.RefreshToken);
    }

    #endregion

    #region Invalid Token Scenarios

    [Fact]
    public async Task RefreshToken_WithNonexistentToken_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: "nonexistent-token-12345");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: "");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    [Fact]
    public async Task RefreshToken_WithNullToken_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: null!);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ReturnsBadRequest()
    {
        // Arrange
        var login = await LoginTestUserAsync("revokedrefresh", "Revoked@123!");
        var client = CreateHttpClient(login.AccessToken);

        // First, logout to revoke the token
        var logoutRequest = new LogoutCommand(
            UserId: login.User.Id,
            RefreshToken: login.RefreshToken);
        await client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Act - Try to refresh with revoked token
        var refreshClient = CreateHttpClient(); // New client without auth header
        var refreshRequest = new RefreshTokenCommand(RefreshToken: login.RefreshToken);
        var response = await refreshClient.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid, expired, or revoked");
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var login = await LoginTestUserAsync("expiredrefresh", "Expired@123!");

        // Create an expired token manually
        var expiredToken = await CreateExpiredRefreshTokenAsync(login.User.Username);

        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: expiredToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid, expired, or revoked");
    }

    [Fact]
    public async Task RefreshToken_WithAccessToken_ReturnsBadRequest()
    {
        // Arrange
        var login = await LoginTestUserAsync("wrongtoken", "Wrong@123!");
        var client = CreateHttpClient();

        // Try to use access token instead of refresh token
        var request = new RefreshTokenCommand(RefreshToken: login.AccessToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenFormat_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: "invalid.format.token");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    [Fact]
    public async Task RefreshToken_WithWhitespaceToken_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: "   token-with-spaces   ");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Be("Refresh token is invalid or not found");
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task RefreshToken_ResponseIncludesAllFields()
    {
        // Arrange
        var login = await LoginTestUserAsync("fielduser", "Field@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.AccessToken.Should().NotBeNull();
        refreshResponse.RefreshToken.Should().NotBeNull();
        refreshResponse.ExpiresInSeconds.Should().BeGreaterThan(0);
        refreshResponse.TokenType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_TokenTypeIsBearer()
    {
        // Arrange
        var login = await LoginTestUserAsync("beareruser", "Bearer@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.TokenType.Should().Be("Bearer");
    }

    #endregion

    #region Database State

    [Fact]
    public async Task RefreshToken_OldTokenIsMarkedAsRevoked()
    {
        // Arrange
        var login = await LoginTestUserAsync("revokeoldtoken", "Revoke@123!");
        var client = CreateHttpClient();
        var oldRefreshToken = login.RefreshToken;

        var request = new RefreshTokenCommand(RefreshToken: oldRefreshToken);

        // Act
        await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert - Check that old token is marked as revoked
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(oldRefreshToken), CancellationToken.None);

        user.Should().NotBeNull();
        var oldToken = user!.RefreshTokens.FirstOrDefault(t => t.Token == oldRefreshToken);
        oldToken.Should().NotBeNull();
        oldToken!.IsRevoked.Should().BeTrue();
        oldToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_NewTokenIsAddedToUser()
    {
        // Arrange
        var login = await LoginTestUserAsync("addnewtoken", "Add@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Assert - Check that new token is added to user
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(refreshResponse!.RefreshToken), CancellationToken.None);

        user.Should().NotBeNull();
        user!.RefreshTokens.Should().Contain(t => t.Token == refreshResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_DoesNotAffectOtherUserTokens()
    {
        // Arrange
        var client = CreateHttpClient();
        var login1 = await LoginTestUserAsync("user1token", "User1@123!");
        var login2 = await LoginTestUserAsync("user2token", "User2@123!");

        var request = new RefreshTokenCommand(RefreshToken: login1.RefreshToken);

        // Act - Refresh user1's token
        await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert - Verify user2's token is still active
        var userRepository = GetService<IUserRepository>();
        var user2 = await userRepository.FindAsync(new GetUserByRefreshToken(login2.RefreshToken), CancellationToken.None);

        user2.Should().NotBeNull();
        var token2 = user2!.RefreshTokens.FirstOrDefault(t => t.Token == login2.RefreshToken);
        token2.Should().NotBeNull();
        token2!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_OnlyRevokesSpecificToken()
    {
        // Arrange
        var client = CreateHttpClient();
        var username = "multitokenuser";

        // Login multiple times to get multiple tokens
        var login1 = await LoginTestUserAsync(username, "Multi@123!");
        await Task.Delay(100);
        var login2 = await LoginTestUserAsync(username, "Multi@123!");

        var request = new RefreshTokenCommand(RefreshToken: login1.RefreshToken);

        // Act - Refresh only first token
        await client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert - Verify only first token is revoked, second is still active
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(login2.RefreshToken), CancellationToken.None);

        user.Should().NotBeNull();

        var token1 = user!.RefreshTokens.FirstOrDefault(t => t.Token == login1.RefreshToken);
        token1.Should().NotBeNull();
        token1!.IsRevoked.Should().BeTrue();

        var token2 = user.RefreshTokens.FirstOrDefault(t => t.Token == login2.RefreshToken);
        token2.Should().NotBeNull();
        token2!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_UserCanContinueUsingNewToken()
    {
        // Arrange
        var login = await LoginTestUserAsync("continueuser", "Continue@123!");
        var client = CreateHttpClient();
        var request = new RefreshTokenCommand(RefreshToken: login.RefreshToken);

        // Act - First refresh
        var response = await client.PostAsJsonAsync("/api/auth/refresh", request);
        var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Act - Second refresh with new token
        var request2 = new RefreshTokenCommand(RefreshToken: refreshResponse!.RefreshToken);
        var response2 = await client.PostAsJsonAsync("/api/auth/refresh", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse2 = await response2.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        refreshResponse2.Should().NotBeNull();
        refreshResponse2!.AccessToken.Should().NotBeEmpty();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an expired refresh token for testing.
    /// </summary>
    private async Task<string> CreateExpiredRefreshTokenAsync(string username)
    {
        if (_webFactory?.Services == null)
            throw new InvalidOperationException("WebApplicationFactory has been disposed!");

        using var scope = _webFactory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await userRepository.FindAsync(new GetUserByUsername(username), CancellationToken.None);
        user.Should().NotBeNull();

        // Create an expired token
        var expiredTokenString = $"expired-token-{Guid.NewGuid()}";
        var refreshToken = RefreshToken.Create(
            expiredTokenString,
            DateTime.UtcNow.AddDays(1));
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1); // Manually expire the token

        user!.AddRefreshToken(refreshToken);
        await userRepository.UpdateAsync(user, CancellationToken.None);
        await unitOfWork.SaveChangesAsync();

        return expiredTokenString;
    }

    #endregion
}
