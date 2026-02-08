using System.Net;
using System.Net.Http.Json;
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
/// Integration tests for the Logout endpoint.
/// Tests the complete flow: HTTP request → API → Handler → Domain → Database.
/// </summary>
public class LogoutEndpointTests(TestWebApplicationFactory factory) : BaseAuthApiTests(factory)
{
    #region Success Scenarios

    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsLogoutResponse()
    {
        // Arrange
        var login = await LoginTestUserAsync("logoutuser", "Logout@123!");
        var client = CreateHttpClient(login.AccessToken);
        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);
        var logoutResponse = await response.Content.ReadFromJsonAsync<LogoutResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        logoutResponse.Should().NotBeNull();
        logoutResponse!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_RevokesToken()
    {
        // Arrange
        var username = "revokeuser";
        var login = await LoginTestUserAsync(username, "Revoke@123!");
        var client = CreateHttpClient(login.AccessToken);
        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // Act
        await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert - Verify token is revoked in database
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(login.RefreshToken), CancellationToken.None);
        user.Should().NotBeNull();
        
        var revokedToken = user!.RefreshTokens.FirstOrDefault(t => t.Token == login.RefreshToken);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_MultipleTimes_OnlyFirstSucceeds()
    {
        // Arrange
        var login = await LoginTestUserAsync("multilogout", "Multi@123!");
        var client = CreateHttpClient(login.AccessToken);
        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // Act
        var response1 = await client.PostAsJsonAsync("/api/auth/logout", request);
        var response2 = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Invalid Token Scenarios

    [Fact]
    public async Task Logout_WithNonexistentToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var request = new LogoutCommand(UserId: Guid.NewGuid(), RefreshToken: "nonexistent-token-12345");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Contain("User not found");
    }

    [Fact]
    public async Task Logout_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var request = new LogoutCommand(UserId: Guid.NewGuid(), RefreshToken: "");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Contain("User not found");
    }

    [Fact]
    public async Task Logout_WithNullToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var request = new LogoutCommand(UserId: Guid.NewGuid(), RefreshToken: null!);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_WithRevokedToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login = await LoginTestUserAsync("alreadyrevoked", "Already@123!");

        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // First logout
        await client.PostAsJsonAsync("/api/auth/logout", request);

        // Act - Try to logout again with same token
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
        result!.Details.Should().Contain("Refresh token is already revoked");
    }

    [Fact]
    public async Task Logout_WithExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login = await LoginTestUserAsync("expireduser", "Expired@123!");

        // Create an expired token manually
        var expiredToken = await CreateExpiredRefreshTokenAsync(login.User.Username);
        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: expiredToken);
        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ResultError>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_WithAccessToken_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login = await LoginTestUserAsync("wrongtoken", "Wrong@123!");

        // Try to use access token instead of refresh token
        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.AccessToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task Logout_ResponseIncludesMessage()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login = await LoginTestUserAsync("messageuser", "Message@123!");

        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/logout", request);
        var logoutResponse = await response.Content.ReadFromJsonAsync<LogoutResponse>();

        // Assert
        logoutResponse.Should().NotBeNull();
        logoutResponse!.Message.Should().NotBeNullOrEmpty();
        logoutResponse.Message.ToLower().Should().Contain("success");
    }

    #endregion

    #region Database State

    [Fact]
    public async Task Logout_VerifyTokenIsMarkedAsRevoked()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login = await LoginTestUserAsync("dbstateuser", "DbState@123!");

        var request = new LogoutCommand(UserId: login.User.Id, RefreshToken: login.RefreshToken);

        // Act
        await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert - Check database state
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(login.RefreshToken), CancellationToken.None);
        
        user.Should().NotBeNull();
        var token = user!.RefreshTokens.FirstOrDefault(t => t.Token == login.RefreshToken);
        token.Should().NotBeNull();
        token!.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Logout_DoesNotAffectOtherUserTokens()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var login1 = await LoginTestUserAsync("user1logout", "User1@123!");
        var login2 = await LoginTestUserAsync("user2logout", "User2@123!");

        var request = new LogoutCommand(UserId: login1.User.Id, RefreshToken: login1.RefreshToken);

        // Act - Logout user1
        await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert - Verify user2's token is still active
        var userRepository = GetService<IUserRepository>();
        var user2 = await userRepository.FindAsync(new GetUserByRefreshToken(login2.RefreshToken), CancellationToken.None);
        
        user2.Should().NotBeNull();
        var token2 = user2!.RefreshTokens.FirstOrDefault(t => t.Token == login2.RefreshToken);
        token2.Should().NotBeNull();
        token2!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Logout_OnlyRevokesSpecificToken()
    {
        // Arrange
        var client = await CreateAuthenticatedHttpClientForLogout();
        var username = "multitoken";
        
        // Login multiple times to get multiple tokens
        var login1 = await LoginTestUserAsync(username, "Multi@123!");
        await Task.Delay(100);
        var login2 = await LoginTestUserAsync(username, "Multi@123!");

        var request = new LogoutCommand(UserId: login1.User.Id, RefreshToken: login1.RefreshToken);

        // Act - Revoke only first token
        await client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert - Verify only first token is revoked
        var userRepository = GetService<IUserRepository>();
        var user = await userRepository.FindAsync(new GetUserByRefreshToken(login1.RefreshToken), CancellationToken.None);
        
        user.Should().NotBeNull();
        
        var token1 = user!.RefreshTokens.FirstOrDefault(t => t.Token == login1.RefreshToken);
        token1.Should().NotBeNull();
        token1!.IsRevoked.Should().BeTrue();

        var token2 = user.RefreshTokens.FirstOrDefault(t => t.Token == login2.RefreshToken);
        token2.Should().NotBeNull();
        token2!.IsRevoked.Should().BeFalse();
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
