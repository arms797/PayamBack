// PayamBack/Services/Implementations/CacheManager.cs
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class CacheManager : ICacheManager
    {
        private readonly IMemoryCache _cache;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly IMarkazCacheService _markazCache;

        public CacheManager(
            IMemoryCache cache,
            IPermissionCacheService permissionCacheService,
            IMarkazCacheService markazCache)
        {
            _cache = cache;
            _permissionCacheService = permissionCacheService;
            _markazCache = markazCache;
        }

        public void ClearUserCache(int userId)
        {
            _cache.Remove($"UserInfo_{userId}");
        }

        public void ClearMarkazCache()
        {
            _markazCache.ClearCache();
        }

        public void ClearPermissionCache(int? roleId = null)
        {
            if (roleId.HasValue)
                _permissionCacheService.ClearRoleCache(roleId.Value);
            else
                _permissionCacheService.ClearAllCache();
        }

        public void ClearAll()
        {
            // پاک کردن کش‌های معروف
            _cache.Remove("AllMarkazList");
            _cache.Remove("AllMarkazDictionary");
            _permissionCacheService.ClearAllCache();
            // نمی‌توانیم همه کلیدهای UserInfo را پاک کنیم، اما با CacheManager می‌توانیم
        }
    }
}