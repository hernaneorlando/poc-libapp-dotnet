namespace Auth.Application.Common.Security;

public record JwtSettings
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string SecretKey { get; set; }
    public int TokenExpiryInMinutes { get; set; }
    public int RefreshTokenExpiryInDays { get; set; }
    public int RefreshTokenSlidingExpiryInMinutes { get; set; }
}
