namespace LibraryApp.Web.Model.AuthManagement;

public sealed record LoginRequestDto(
    string Username,
    string Password,
    bool RememberMe = false
);
