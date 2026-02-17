namespace LibraryApp.Web.Model.Auth;

public sealed record LogoutRequestDto(
    long ExternalId,
    string RefreshToken
);
