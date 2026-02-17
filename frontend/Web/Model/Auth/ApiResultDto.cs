namespace LibraryApp.Web.Model.Auth;

/// <summary>
/// Represents the Result pattern response from the backend API.
/// Matches the Core.API.Result<T> structure.
/// </summary>
public sealed record ApiResultDto<T>
{
    public T? Value { get; init; }
    public List<ErrorDto> Errors { get; init; } = [];
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
}

public sealed record ErrorDto(
    string Title,
    string Detail,
    int Status,
    string[] Errors
);
