namespace Application.Auth;

public sealed class TokenInfo
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public string? Jti { get; set; }
    public DateTime? Expiration { get; set; }

    public Guid GetUserId()
    {
        return Guid.TryParse(UserId, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authenticated user id is missing.");
    }
}
