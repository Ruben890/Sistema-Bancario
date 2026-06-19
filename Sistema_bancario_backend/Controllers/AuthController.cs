using System.Net;
using Application.Abstractions;
using Application.Auth;
using Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_bancario_backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IJwtTokenBlacklist jwtTokenBlacklist,
    IAuthCookieService authCookieService,
    TokenInfo tokenInfo) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return new ObjectResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await authService.GetCurrentUserAsync(tokenInfo.GetUserId(), cancellationToken);
        return new ObjectResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (string.IsNullOrWhiteSpace(tokenInfo.UserId) ||
            string.IsNullOrWhiteSpace(tokenInfo.Jti) ||
            !tokenInfo.Expiration.HasValue)
        {
            return new ObjectResult(Result<object>.Failure(
                "TOKEN_INVALID",
                "Token data is invalid.",
                HttpStatusCode.BadRequest));
        }

        jwtTokenBlacklist.LogoutToken(tokenInfo.UserId, tokenInfo.Jti, tokenInfo.Expiration.Value);
        authCookieService.ClearAccessToken();

        return new ObjectResult(Result<object>.Response("Logout completed successfully.", HttpStatusCode.OK));
    }
}
