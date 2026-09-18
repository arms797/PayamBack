// PayamBack/Services/Implementations/CacheVersionManager.cs
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

public class CacheVersionManager
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _roleTokens = new();

    public CacheVersionManager(IMemoryCache cache)
    {
        _cache = cache;
    }

    // گرفتن توکن تغییر برای یک نقش
    public CancellationToken GetRoleToken(int roleId)
    {
        var cts = _roleTokens.GetOrAdd(roleId, _ => new CancellationTokenSource());
        return cts.Token;
    }

    // باطل کردن کش یک نقش (با ارسال سیگنال تغییر)
    public void InvalidateRole(int roleId)
    {
        if (_roleTokens.TryRemove(roleId, out var oldCts))
        {
            oldCts.Cancel(); // سیگنال تغییر
            oldCts.Dispose();
        }
    }
}