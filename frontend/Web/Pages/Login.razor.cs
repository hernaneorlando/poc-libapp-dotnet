using Microsoft.AspNetCore.Components;
using LibraryApp.Web.Services.Auth;

namespace LibraryApp.Web.Pages;

public partial class Login
{
    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string Username = string.Empty;
    private string Password = string.Empty;
    private bool RememberMe = false;
    private bool IsLoading = false;
    private string ErrorMessage = string.Empty;

    private async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please fill in username and password.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var (success, error) = await AuthService.LoginAsync(Username, Password, RememberMe);
            
            if (success)
            {
                // Redirect to home page after successful login
                Navigation.NavigateTo("/");
            }
            else
            {
                ErrorMessage = error ?? "Login failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
