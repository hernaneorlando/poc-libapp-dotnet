namespace LibraryApp.Web.Model.AuthManagement;

public sealed record RefreshTokenRequestDto(
    string RefreshToken
);

public sealed record RefreshTokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType
);
