using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace PayamBack.Services.Implementations
{
    public class TrackingMemoryCache : IMemoryCache
    {
        private readonly IMemoryCache _inner;
        private readonly ConcurrentDictionary<string, byte> _trackedKeys = new();

        public TrackingMemoryCache(IMemoryCache inner)
        {
            _inner = inner;
        }

        public ICacheEntry CreateEntry(object key)
        {
            var keyStr = key?.ToString();
            if (!string.IsNullOrEmpty(keyStr))
                _trackedKeys.TryAdd(keyStr, 0);
            return _inner.CreateEntry(key);
        }

        public void Remove(object key)
        {
            var keyStr = key?.ToString();
            if (!string.IsNullOrEmpty(keyStr))
                _trackedKeys.TryRemove(keyStr, out _);
            _inner.Remove(key);
        }

        public bool TryGetValue(object key, out object? value)
        {
            return _inner.TryGetValue(key, out value);
        }

        public void Dispose()
        {
            _inner.Dispose();
            GC.SuppressFinalize(this);
        }

        public int ClearByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return 0;

            var keysToRemove = _trackedKeys.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _inner.Remove(key);
                _trackedKeys.TryRemove(key, out _);
            }
            return keysToRemove.Count;
        }
    }
}