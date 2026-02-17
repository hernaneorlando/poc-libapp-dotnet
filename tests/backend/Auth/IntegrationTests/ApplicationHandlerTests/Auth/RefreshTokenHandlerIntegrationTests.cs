using Auth.Application.Users.Commands.RefreshToken;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Core.API;
using Microsoft.EntityFrameworkCore;

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
public class RefreshTokenHandlerIntegrationTests : BaseHandlerTests
{
    #region Successful Refresh Scenarios

    [Fact]
    public async Task Handle_ValidRefreshToken_WithNoRememberMe_ReturnsNewToken()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync(rememberMe: false);
        
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RefreshTokenResponse>.Success>();
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.AccessToken.Should().NotBe(loginResponse.AccessToken);
        successResult.Data.RefreshToken.Should().NotBeNullOrEmpty();
        successResult.Data.RefreshToken.Should().NotBe(loginResponse.RefreshToken);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_WithRememberMe_ReturnsNewToken()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync(rememberMe: true);
        
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RefreshTokenResponse>.Success>();
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.AccessToken.Should().NotBe(loginResponse.AccessToken);
        successResult.Data.RefreshToken.Should().NotBeNullOrEmpty();
        successResult.Data.RefreshToken.Should().NotBe(loginResponse.RefreshToken);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_OldTokenIsRevoked()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        await _mediator.Send(command);

        // Assert - Verify old token is revoked
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

        var oldToken = updatedUser!.RefreshTokens
            .FirstOrDefault(rt => rt.Token == loginResponse.RefreshToken);
        
        oldToken.Should().NotBeNull();
        oldToken!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_NewTokenIsAdded()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        var newRefreshToken = successResult.Data.RefreshToken;

        // Assert - Verify new token is in database
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

        var newToken = updatedUser!.RefreshTokens
            .FirstOrDefault(rt => rt.Token == newRefreshToken);
        
        newToken.Should().NotBeNull();
        newToken!.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_TokenCountIncreases()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        
        _dbContext.ChangeTracker.Clear();
        var userBefore = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);
        var tokenCountBefore = userBefore!.RefreshTokens.Count;

        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        await _mediator.Send(command);

        // Assert
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var userAfter = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

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
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        
        // First refresh to revoke the old token
        var firstRefreshCommand = new RefreshTokenCommand(loginResponse.RefreshToken);
        await _mediator.Send(firstRefreshCommand);

        // Try to refresh with the now-revoked token
        var secondRefreshCommand = new RefreshTokenCommand(loginResponse.RefreshToken);

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
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        var token1 = loginResponse.RefreshToken;

        // Add second token to domain model
        var domainUser = await _userRepository.GetByIdAsync(UserTest.Id);
        var token2String = _tokenService.GenerateRefreshToken(UserTest.Id.Value);
        var token2 = RefreshToken.Create(token2String, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays));
        domainUser!.AddRefreshToken(token2);
        await _userRepository.UpdateAsync(domainUser);
        await _dbContext.SaveChangesAsync();

        // Refresh with first token
        var command = new RefreshTokenCommand(token1);

        // Act
        await _mediator.Send(command);

        // Assert - First token should be revoked, second token should still be valid
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

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
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        var token1 = loginResponse.RefreshToken;

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
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

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
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;

        // Assert - Access token should be non-empty (format validation would require JWT parsing)
        successResult.Data.AccessToken.Should().NotBeNullOrEmpty();
        successResult.Data.AccessToken.Should().Contain(".");  // JWT format check
    }

    [Fact]
    public async Task HandleRefreshWithNoRememberMe_NewRefreshTokenHasCorrectExpiry()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync();
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        var timeBeforeRefresh = DateTime.UtcNow;

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        var newRefreshToken = successResult.Data.RefreshToken;

        var timeAfterRefresh = DateTime.UtcNow;

        // Assert - New token should exist in database with correct expiry
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

        var token = updatedUser!.RefreshTokens.FirstOrDefault(rt => rt.Token == newRefreshToken);
        token.Should().NotBeNull();

        var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenSlidingExpiryInMinutes);
        token!.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task HandleRefreshWithRememberMe_NewRefreshTokenHasCorrectExpiry()
    {
        // Arrange
        var loginResponse = await CreateUserWithRefreshTokenAsync(rememberMe: true);
        var command = new RefreshTokenCommand(loginResponse.RefreshToken);

        var timeBeforeRefresh = DateTime.UtcNow;

        // Act
        var result = await _mediator.Send(command);
        var successResult = (Result<RefreshTokenResponse>.Success)result;
        var newRefreshToken = successResult.Data.RefreshToken;

        var timeAfterRefresh = DateTime.UtcNow;

        // Assert - New token should exist in database with correct expiry
        await Task.Delay(50);  // Ensure transaction is committed
        _dbContext.ChangeTracker.Clear();
        
        var updatedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == UserTest.Id.Value);

        var token = updatedUser!.RefreshTokens.FirstOrDefault(rt => rt.Token == newRefreshToken);
        token.Should().NotBeNull();

        var expectedExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
        token!.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    #endregion
}
