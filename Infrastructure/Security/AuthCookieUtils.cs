using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Security;

public static class AuthCookieUtils
{
    public static CookieOptions BuildCreateCookieOptions(
        HttpContext context,
        AuthCookieOptions authCookieOptions,
        DateTimeOffset expires)
    {
        var isProduction = context.RequestServices.GetRequiredService<IHostEnvironment>().IsProduction();

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = authCookieOptions.SecureInProductionOnly ? isProduction : context.Request.IsHttps,
            SameSite = ParseSameSite(authCookieOptions.SameSite),
            Path = "/",
            Expires = expires
        };

        if (ShouldUseDomain(context, authCookieOptions, isProduction))
            options.Domain = authCookieOptions.Domain;

        return options;
    }

    public static CookieOptions BuildDeleteCookieOptions(HttpContext context, AuthCookieOptions authCookieOptions)
    {
        var options = BuildCreateCookieOptions(context, authCookieOptions, DateTimeOffset.UtcNow.AddDays(-1));
        options.Expires = DateTimeOffset.UtcNow.AddDays(-1);
        return options;
    }

    public static void ClearAuthCookie(HttpContext context, AuthCookieOptions authCookieOptions)
        => context.Response.Cookies.Delete(
            authCookieOptions.AccessTokenCookieName,
            BuildDeleteCookieOptions(context, authCookieOptions));

    private static bool ShouldUseDomain(HttpContext context, AuthCookieOptions options, bool isProduction)
    {
        if (string.IsNullOrWhiteSpace(options.Domain))
            return false;

        if (options.UseDomainInProductionOnly && !isProduction)
            return false;

        var host = context.Request.Host.Host;
        return !host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
               && !host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }

    private static SameSiteMode ParseSameSite(string? sameSite)
        => sameSite?.Trim().ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "strict" => SameSiteMode.Strict,
            _ => SameSiteMode.Lax
        };
}
