namespace LibraryApp.Web.Model.Auth;

public sealed record RefreshTokenRequestDto(
    string RefreshToken
);

public sealed record RefreshTokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType
);
