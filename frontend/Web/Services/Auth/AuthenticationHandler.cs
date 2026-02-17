using System.Net;
using System.Net.Http.Headers;

namespace LibraryApp.Web.Services.Auth;

/// <summary>
/// HTTP message handler that automatically adds the Authorization header with JWT token
/// and handles 401 Unauthorized responses by attempting to refresh the token.
/// </summary>
public sealed class AuthenticationHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthService _authService;
    private bool _isRefreshing = false;

    public AuthenticationHandler(
        ITokenStorageService tokenStorage,
        IAuthService authService)
    {
        _tokenStorage = tokenStorage;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Skip adding token for auth endpoints
        if (IsAuthEndpoint(request.RequestUri))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Add Authorization header if token exists
        var accessToken = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // Send request
        var response = await base.SendAsync(request, cancellationToken);

        // Handle 401 Unauthorized - attempt token refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            _isRefreshing = true;
            
            try
            {
                // Attempt to refresh the token
                var refreshed = await _authService.RefreshTokenAsync();
                if (refreshed)
                {
                    // Get new access token
                    var newAccessToken = await _tokenStorage.GetAccessTokenAsync();
                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        // Clone the original request (we can't reuse the original)
                        var clonedRequest = await CloneHttpRequestMessageAsync(request);
                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                        
                        // Retry the request with new token
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                    }
                }
                else
                {
                    // Refresh failed - logout user
                    await _authService.LogoutAsync();
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response;
    }

    private static bool IsAuthEndpoint(Uri? requestUri)
    {
        if (requestUri == null) return false;
        
        var path = requestUri.AbsolutePath.ToLowerInvariant();
        return path.Contains("/api/auth/login") 
            || path.Contains("/api/auth/logout") 
            || path.Contains("/api/auth/refresh");
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
