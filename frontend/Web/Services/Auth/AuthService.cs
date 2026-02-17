using LibraryApp.Web.Model.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryApp.Web.Services.Auth;

/// <summary>
/// Service for handling authentication operations.
/// Communicates with the backend API for login, logout, and token refresh.
/// Token expiration is handled transparently by AuthenticationHandler (401 auto-refresh).
/// </summary>
public interface IAuthService
{
    Task<(bool Success, string? Error)> LoginAsync(string username, string password, bool rememberMe);
    Task LogoutAsync();
    Task<bool> RefreshTokenAsync();
    Task<bool> RestoreSessionAsync();
}

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthStateService _authState;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(
        HttpClient httpClient,
        ITokenStorageService tokenStorage,
        IAuthStateService authState)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        _authState = authState;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string username, string password, bool rememberMe)
    {
        try
        {
            var request = new LoginRequestDto(username, password, rememberMe);
            
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"Login failed with status {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResultDto<LoginResponseDto>>(_jsonOptions);
            
            if (result == null || result.IsFailure || result.Value == null)
            {
                var error = result?.Errors.FirstOrDefault()?.Detail ?? "Login failed";
                return (false, error);
            }

            var loginResponse = result.Value!;

            await _tokenStorage.SaveTokensAsync(
                loginResponse.AccessToken,
                loginResponse.RefreshToken,
                rememberMe);
            
            // Update global auth state
            _authState.SetUser(loginResponse.User);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Login error: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            var currentUser = _authState.CurrentUser;
            
            if (currentUser != null && !string.IsNullOrEmpty(refreshToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new LogoutRequestDto(currentUser.ExternalId, refreshToken);
                
                // Call backend to revoke refresh token (don't wait for response)
                _ = _httpClient.PostAsJsonAsync("/api/auth/logout", request);
            }
        }
        finally
        {
            // Always clear local state
            await _tokenStorage.ClearTokensAsync();
            _authState.ClearUser();
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var request = new RefreshTokenRequestDto(refreshToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", request);
            
            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResultDto<RefreshTokenResponseDto>>(_jsonOptions);
            
            if (result == null || result.IsFailure || result.Value == null)
            {
                await LogoutAsync();
                return false;
            }

            var refreshResponse = result.Value!;
            
            // Update stored tokens (preserve rememberMe setting)
            var hasTokens = await _tokenStorage.HasTokensAsync();
            if (hasTokens)
            {
                // Determine if using localStorage (rememberMe) or sessionStorage
                var storageType = await GetCurrentStorageTypeAsync();
                var rememberMe = storageType == "localStorage";
                
                await _tokenStorage.SaveTokensAsync(
                    refreshResponse.AccessToken,
                    refreshResponse.RefreshToken,
                    rememberMe);
            }
            
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            var accessToken = await _tokenStorage.GetAccessTokenAsync();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            // Decode JWT to get user info (without verification - backend will validate)
            var userInfo = DecodeJwtPayload(accessToken);
            if (userInfo != null)
            {
                _authState.SetUser(userInfo);
                return true;
            }

            // If can't decode, try to refresh token
            return await RefreshTokenAsync();
        }
        catch
        {
            await _tokenStorage.ClearTokensAsync();
            return false;
        }
    }

    private UserLoginInfoDto? DecodeJwtPayload(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            
            // Add padding if needed
            var padding = payload.Length % 4;
            if (padding > 0)
            {
                payload += new string('=', 4 - padding);
            }

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson, _jsonOptions);
            
            if (claims == null) return null;

            // Extract claims from JWT
            var externalId = claims.TryGetValue("externalId", out JsonElement value3) 
                ? value3.GetInt64() 
                : 0L;
            
            var username = claims.TryGetValue("sub", out JsonElement value2) 
                ? value2.GetString() ?? string.Empty 
                : string.Empty;
            
            var email = claims.TryGetValue("email", out JsonElement value1) 
                ? value1.GetString() ?? string.Empty 
                : string.Empty;
            
            var fullName = claims.TryGetValue("name", out JsonElement value) 
                ? value.GetString() ?? string.Empty 
                : string.Empty;

            var roles = new List<string>();
            if (claims.TryGetValue("role", out JsonElement roleElement))
            {
                if (roleElement.ValueKind == JsonValueKind.Array)
                {
                    roles = [.. roleElement.EnumerateArray()
                        .Select(r => r.GetString() ?? string.Empty)
                        .Where(r => !string.IsNullOrEmpty(r))];
                }
                else if (roleElement.ValueKind == JsonValueKind.String)
                {
                    var role = roleElement.GetString();
                    if (!string.IsNullOrEmpty(role))
                    {
                        roles.Add(role);
                    }
                }
            }

            // Extract permissions from JWT (format: "Feature:Action")
            var permissions = new List<string>();
            if (claims.TryGetValue("permission", out JsonElement permissionElement))
            {
                if (permissionElement.ValueKind == JsonValueKind.Array)
                {
                    permissions = [.. permissionElement.EnumerateArray()
                        .Select(p => p.GetString() ?? string.Empty)
                        .Where(p => !string.IsNullOrEmpty(p))];
                }
                else if (permissionElement.ValueKind == JsonValueKind.String)
                {
                    var permission = permissionElement.GetString();
                    if (!string.IsNullOrEmpty(permission))
                    {
                        permissions.Add(permission);
                    }
                }
            }

            return new UserLoginInfoDto(externalId, username, email, fullName, roles, permissions);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetCurrentStorageTypeAsync()
    {
        try
        {
            // Check if tokens exist to determine storage type
            var hasTokens = await _tokenStorage.HasTokensAsync();
            return hasTokens ? "localStorage" : "sessionStorage";
        }
        catch
        {
            return "sessionStorage";
        }
    }
}
