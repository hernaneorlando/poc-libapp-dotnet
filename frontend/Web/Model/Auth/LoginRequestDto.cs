namespace LibraryApp.Web.Model.Auth;

public sealed record LoginRequestDto(
    string Username,
    string Password,
    bool RememberMe = false
);
