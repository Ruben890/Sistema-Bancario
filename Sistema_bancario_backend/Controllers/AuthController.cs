using Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_bancario_backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
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
        var result = authService.Logout();
        return new ObjectResult(result);
    }
}
