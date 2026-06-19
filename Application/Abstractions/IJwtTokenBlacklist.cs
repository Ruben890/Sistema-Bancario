namespace Application.Abstractions;

public interface IJwtTokenBlacklist
{
    void AddToWhiteList(string userId, string jti, DateTime expiration);
    bool IsTokenAllowed(string userId, string jti);
    bool IsWhiteListed(string userId, string jti);
    bool IsBlackListed(string jti);
    void LogoutToken(string userId, string jti, DateTime expiration);
    void BlockToken(string userId, string jti, DateTime expiration);
    void BlockAllUserTokens(string userId);
    void RemoveFromWhiteList(string userId, string jti);
}
