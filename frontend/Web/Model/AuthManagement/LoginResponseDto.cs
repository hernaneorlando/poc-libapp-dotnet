using LibraryApp.Web.Model.AuthManagement.Enums;

namespace LibraryApp.Web.Model.AuthManagement;

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
    UserType UserType,
    List<RoleInfo> Roles
);

public sealed record RoleInfo(
    string Name,
    List<string> Permissions
);