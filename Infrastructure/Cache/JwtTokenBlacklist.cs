using System.Collections.Concurrent;
using Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Cache;

public sealed class JwtTokenBlacklist(IMemoryCache memoryCache) : IJwtTokenBlacklist
{
    private const string WhiteListPrefix = "jwt:white-list:";
    private const string BlackListPrefix = "jwt:black-list:";

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JwtTokenCacheItem>> _userTokens = new();

    public void AddToWhiteList(string userId, string jti, DateTime expiration)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        var expiresUtc = NormalizeExpiration(expiration);
        if (expiresUtc <= DateTime.UtcNow)
            return;

        var item = new JwtTokenCacheItem(userId, jti, expiresUtc);
        var options = new MemoryCacheEntryOptions { AbsoluteExpiration = expiresUtc };
        options.RegisterPostEvictionCallback((_, value, _, _) =>
        {
            if (value is JwtTokenCacheItem tokenItem)
                RemoveFromUserIndex(tokenItem.UserId, tokenItem.Jti);
        });

        memoryCache.Set(GetWhiteListKey(userId, jti), item, options);

        var userIndex = _userTokens.GetOrAdd(userId, _ => new ConcurrentDictionary<string, JwtTokenCacheItem>());
        userIndex[jti] = item;
    }

    public bool IsTokenAllowed(string userId, string jti)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        if (IsBlackListed(jti))
            return false;

        return IsWhiteListed(userId, jti);
    }

    public bool IsWhiteListed(string userId, string jti)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        return memoryCache.TryGetValue<JwtTokenCacheItem>(
            GetWhiteListKey(userId, jti),
            out var tokenItem) && tokenItem is not null && tokenItem.Expiration > DateTime.UtcNow;
    }

    public bool IsBlackListed(string jti)
    {
        ValidateJti(jti);

        return memoryCache.TryGetValue<JwtBlackListCacheItem>(
            GetBlackListKey(jti),
            out var item) && item is not null && item.Expiration > DateTime.UtcNow;
    }

    public void LogoutToken(string userId, string jti, DateTime expiration)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        var expiresUtc = NormalizeExpiration(expiration);
        RemoveFromWhiteList(userId, jti);
        AddToBlackList(jti, expiresUtc, JwtBlackListReason.Logout);
    }

    public void BlockToken(string userId, string jti, DateTime expiration)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        var expiresUtc = NormalizeExpiration(expiration);
        RemoveFromWhiteList(userId, jti);
        AddToBlackList(jti, expiresUtc, JwtBlackListReason.ManualBlock);
    }

    public void BlockAllUserTokens(string userId)
    {
        ValidateUserId(userId);

        if (!_userTokens.TryGetValue(userId, out var userIndex))
            return;

        foreach (var token in userIndex.Values.ToList())
        {
            RemoveFromWhiteList(token.UserId, token.Jti);
            AddToBlackList(token.Jti, token.Expiration, JwtBlackListReason.UserAccessRevoked);
        }

        _userTokens.TryRemove(userId, out _);
    }

    public void RemoveFromWhiteList(string userId, string jti)
    {
        ValidateUserId(userId);
        ValidateJti(jti);

        memoryCache.Remove(GetWhiteListKey(userId, jti));
        RemoveFromUserIndex(userId, jti);
    }

    private void AddToBlackList(string jti, DateTime expiration, JwtBlackListReason reason)
    {
        var expiresUtc = NormalizeExpiration(expiration);
        if (expiresUtc <= DateTime.UtcNow)
            return;

        memoryCache.Set(
            GetBlackListKey(jti),
            new JwtBlackListCacheItem(jti, expiresUtc, reason, DateTime.UtcNow),
            new MemoryCacheEntryOptions { AbsoluteExpiration = expiresUtc });
    }

    private void RemoveFromUserIndex(string userId, string jti)
    {
        if (!_userTokens.TryGetValue(userId, out var userIndex))
            return;

        userIndex.TryRemove(jti, out _);
        if (userIndex.IsEmpty)
            _userTokens.TryRemove(userId, out _);
    }

    private static string GetWhiteListKey(string userId, string jti)
        => $"{WhiteListPrefix}{userId}:{jti}";

    private static string GetBlackListKey(string jti)
        => $"{BlackListPrefix}{jti}";

    private static DateTime NormalizeExpiration(DateTime expiration)
        => expiration.Kind switch
        {
            DateTimeKind.Utc => expiration,
            DateTimeKind.Local => expiration.ToUniversalTime(),
            _ => DateTime.SpecifyKind(expiration, DateTimeKind.Utc)
        };

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
    }

    private static void ValidateJti(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("Jti is required.", nameof(jti));
    }

    private sealed record JwtTokenCacheItem(string UserId, string Jti, DateTime Expiration);
    private sealed record JwtBlackListCacheItem(string Jti, DateTime Expiration, JwtBlackListReason Reason, DateTime CreatedAt);

    private enum JwtBlackListReason
    {
        Logout = 1,
        ManualBlock = 2,
        UserAccessRevoked = 3
    }
}
