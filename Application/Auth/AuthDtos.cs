namespace Application.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record AuthResponse(Guid UserId, string Name, string Email, string Role, string Token);
