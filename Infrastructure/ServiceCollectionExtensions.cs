using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions;
using Application.Auth;
using Infrastructure.Cache;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPatternOptions(configuration);
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthCookieService, AuthCookieService>();
        services.AddScoped<TokenInfo>();
        services.AddSingleton<ILoanStatusCache, LoanStatusCache>();
        services.AddSingleton<IJwtTokenBlacklist, JwtTokenBlacklist>();

        var jwtOptions = configuration.GetRequiredSection("Jwt").Get<JwtOptions>()
                         ?? throw new InvalidOperationException("Jwt configuration is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookieOptions = context.HttpContext.RequestServices
                            .GetRequiredService<IOptions<AuthCookieOptions>>()
                            .Value;

                        if (context.Request.Cookies.TryGetValue(
                                cookieOptions.AccessTokenCookieName,
                                out var accessToken)
                            && !string.IsNullOrWhiteSpace(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var userId = GetUserId(context.Principal);
                        var role = GetRole(context.Principal);
                        var jti = GetJti(context.Principal);

                        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jti))
                        {
                            context.Fail("Token does not contain userId or jti.");
                            return Task.CompletedTask;
                        }

                        var blacklist = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenBlacklist>();
                        if (!blacklist.IsTokenAllowed(userId, jti))
                        {
                            ClearAuthCookie(context.HttpContext);
                            context.Fail("Token is no longer allowed.");
                            return Task.CompletedTask;
                        }

                        var tokenInfo = context.HttpContext.RequestServices.GetRequiredService<TokenInfo>();
                        tokenInfo.UserId = userId;
                        tokenInfo.Role = role;
                        tokenInfo.Jti = jti;
                        tokenInfo.Expiration = GetExpiration(context.Principal);

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        ClearAuthCookie(context.HttpContext);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        ClearAuthCookie(context.HttpContext);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static void ClearAuthCookie(HttpContext context)
    {
        var cookieOptions = context.RequestServices.GetRequiredService<IOptions<AuthCookieOptions>>().Value;
        AuthCookieUtils.ClearAuthCookie(context, cookieOptions);
    }

    private static void AddPatternOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<AuthCookieOptions>(configuration.GetSection("AuthCookies"));
    }

    private static string? GetUserId(ClaimsPrincipal? principal)
        => principal?.FindFirst("userId")?.Value
           ?? principal?.FindFirst("UserId")?.Value
           ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    private static string? GetRole(ClaimsPrincipal? principal)
        => principal?.FindFirst("role")?.Value
           ?? principal?.FindFirst("Role")?.Value
           ?? principal?.FindFirst(ClaimTypes.Role)?.Value;

    private static string? GetJti(ClaimsPrincipal? principal)
        => principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
           ?? principal?.FindFirst("jti")?.Value;

    private static DateTime? GetExpiration(ClaimsPrincipal? principal)
    {
        var value = principal?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
                    ?? principal?.FindFirst("exp")?.Value;

        return long.TryParse(value, out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
            : null;
    }
}
