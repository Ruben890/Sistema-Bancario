using Application.Abstractions;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Cache;

public sealed class LoanStatusCache(IMemoryCache cache) : ILoanStatusCache
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

    public async Task<LoanStatus?> GetOrCreateAsync(Guid loanId, Func<Task<LoanStatus?>> factory)
    {
        var key = BuildKey(loanId);

        if (cache.TryGetValue(key, out LoanStatus status))
            return status;

        var resolvedStatus = await factory();
        if (resolvedStatus.HasValue)
            Set(loanId, resolvedStatus.Value);

        return resolvedStatus;
    }

    public void Set(Guid loanId, LoanStatus status)
        => cache.Set(BuildKey(loanId), status, Expiration);

    public void Remove(Guid loanId)
        => cache.Remove(BuildKey(loanId));

    private static string BuildKey(Guid loanId)
        => $"loan-status:{loanId}";
}
