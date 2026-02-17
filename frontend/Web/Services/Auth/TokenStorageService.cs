using Microsoft.JSInterop;
using System.Text.Json;

namespace LibraryApp.Web.Services.Auth;

/// <summary>
/// Service for managing authentication tokens in browser storage.
/// Uses localStorage for "Remember Me" functionality and sessionStorage for temporary sessions.
/// </summary>
public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task ClearTokensAsync();
    Task<bool> HasTokensAsync();
}

public sealed class TokenStorageService(IJSRuntime _jsRuntime) : ITokenStorageService
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string StorageTypeKey = "auth_storage_type";
    
    public async Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe)
    {
        var storageType = rememberMe ? "localStorage" : "sessionStorage";
        
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"{storageType}.setItem('{AccessTokenKey}', '{accessToken}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"{storageType}.setItem('{RefreshTokenKey}', '{refreshToken}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"{storageType}.setItem('{StorageTypeKey}', '{storageType}')");
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var storageType = await GetStorageTypeAsync();
        if (storageType == null) return null;

        return await _jsRuntime.InvokeAsync<string?>("eval", 
            $"{storageType}.getItem('{AccessTokenKey}')");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        var storageType = await GetStorageTypeAsync();
        if (storageType == null) return null;

        return await _jsRuntime.InvokeAsync<string?>("eval", 
            $"{storageType}.getItem('{RefreshTokenKey}')");
    }

    public async Task ClearTokensAsync()
    {
        // Clear from both storages to ensure cleanup
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"localStorage.removeItem('{AccessTokenKey}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"localStorage.removeItem('{RefreshTokenKey}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"localStorage.removeItem('{StorageTypeKey}')");
        
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"sessionStorage.removeItem('{AccessTokenKey}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"sessionStorage.removeItem('{RefreshTokenKey}')");
        await _jsRuntime.InvokeVoidAsync("eval", 
            $"sessionStorage.removeItem('{StorageTypeKey}')");
    }

    public async Task<bool> HasTokensAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(accessToken);
    }

    private async Task<string?> GetStorageTypeAsync()
    {
        // Check localStorage first
        var localStorageType = await _jsRuntime.InvokeAsync<string?>("eval", 
            $"localStorage.getItem('{StorageTypeKey}')");
        if (localStorageType != null) return localStorageType;

        // Then check sessionStorage
        var sessionStorageType = await _jsRuntime.InvokeAsync<string?>("eval", 
            $"sessionStorage.getItem('{StorageTypeKey}')");
        return sessionStorageType;
    }
}
