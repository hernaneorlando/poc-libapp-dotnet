namespace Core.API;

/// <summary>
/// Standard error response model following ProblemDetails pattern.
/// </summary>
public sealed record ErrorResponse
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public int Status { get; init; }
    public List<string> Errors { get; init; } = [];
}
