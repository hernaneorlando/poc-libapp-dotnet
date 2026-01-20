namespace LibraryApp.API.Extensions;

/// <summary>
/// Result error details for API responses.
/// </summary>
public record ResultError(string Title, string Details, int StatusCode);
