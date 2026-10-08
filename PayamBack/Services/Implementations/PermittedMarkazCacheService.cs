using Microsoft.Extensions.Caching.Memory;
using PayamBack.DTOs.Schedule.BarnamehHaftegi;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class PermittedMarkazCacheService : IPermittedMarkazCacheService
    {
        private readonly TrackingMemoryCache _cache;
        private const string KeyPrefix = "PermittedMarkazInfo_";
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(6);

        public PermittedMarkazCacheService(TrackingMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<PermittedMarkazInfo>> GetAsync(
            int ostadId,
            string? termCode,
            Func<Task<List<PermittedMarkazInfo>>> factory)
        {
            if (string.IsNullOrEmpty(termCode))
                return await factory();

            var key = BuildKey(ostadId, termCode);

            if (_cache.TryGetValue(key, out List<PermittedMarkazInfo>? cached) && cached != null)
                return cached;

            var result = await factory();
            _cache.Set(key, result, DefaultExpiry);
            return result;
        }

        public void Clear(int ostadId, string? termCode)
        {
            if (string.IsNullOrEmpty(termCode))
            {
                ClearAllForOstad(ostadId);
                return;
            }
            _cache.Remove(BuildKey(ostadId, termCode));
        }

        public void ClearAllForOstad(int ostadId)
        {
            _cache.ClearByPrefix($"{KeyPrefix}{ostadId}_");
        }

        private static string BuildKey(int ostadId, string termCode)
            => $"{KeyPrefix}{ostadId}_{termCode}";
    }
}