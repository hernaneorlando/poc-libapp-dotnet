namespace LibraryApp.Web.Model.Auth;

public sealed record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserLoginInfoDto User
);

public sealed record UserLoginInfoDto(
    long ExternalId,
    string Username,
    string Email,
    string FullName,
    List<string> Roles,
    List<string> Permissions
);
