using System.IdentityModel.Tokens.Jwt;
using Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security;

public sealed class AuthCookieService(
    IHttpContextAccessor httpContextAccessor,
    IOptions<AuthCookieOptions> authCookieOptions) : IAuthCookieService
{
    public void WriteAccessToken(string token)
    {
        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is required to write auth cookies.");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expires = jwt.ValidTo == DateTime.MinValue
            ? DateTimeOffset.UtcNow.AddHours(1)
            : new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero);

        httpContext.Response.Cookies.Append(
            authCookieOptions.Value.AccessTokenCookieName,
            token,
            AuthCookieUtils.BuildCreateCookieOptions(httpContext, authCookieOptions.Value, expires));
    }

    public void ClearAccessToken()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        AuthCookieUtils.ClearAuthCookie(httpContext, authCookieOptions.Value);
    }
}
