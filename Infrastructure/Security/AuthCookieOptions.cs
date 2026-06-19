namespace Infrastructure.Security;

public sealed class AuthCookieOptions
{
    public string AccessTokenCookieName { get; set; } = "accessToken";
    public string? Domain { get; set; }
    public bool UseDomainInProductionOnly { get; set; } = true;
    public bool SecureInProductionOnly { get; set; } = true;
    public string SameSite { get; set; } = "Lax";
}
