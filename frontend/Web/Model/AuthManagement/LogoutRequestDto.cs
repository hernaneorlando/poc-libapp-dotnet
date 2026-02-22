namespace LibraryApp.Web.Model.AuthManagement;

public sealed record LogoutRequestDto(
    long ExternalId,
    string RefreshToken
);
