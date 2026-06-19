using Application.Common;

namespace Application.Auth;

public interface IAuthService
{
    Task<Result<object>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<object>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
    Result<object> Logout();
}
