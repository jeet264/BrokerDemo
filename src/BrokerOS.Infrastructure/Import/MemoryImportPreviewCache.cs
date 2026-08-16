using BrokerOS.Application.Import;
using Microsoft.Extensions.Caching.Memory;

namespace BrokerOS.Infrastructure.Import;

/// <summary>
/// Holds preview rows in process memory for ~30 minutes. Tokens are looked up by GUID and then
/// checked against the caller's OrganizationId in ImportService — a leaked token from another
/// brokerage still cannot commit.
/// </summary>
public sealed class MemoryImportPreviewCache : IImportPreviewCache
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _cache;

    public MemoryImportPreviewCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set(ImportPreviewSession session)
    {
        _cache.Set(CacheKey(session.Token), session, TimeToLive);
    }

    public ImportPreviewSession? Get(Guid token)
    {
        return _cache.TryGetValue(CacheKey(token), out ImportPreviewSession? session) ? session : null;
    }

    public void Remove(Guid token)
    {
        _cache.Remove(CacheKey(token));
    }

    private static string CacheKey(Guid token) => $"brokeros:import:{token:N}";
}
