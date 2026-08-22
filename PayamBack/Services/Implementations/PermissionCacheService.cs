// PayamBack/Services/Implementations/PermissionCacheService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "RolePermissions_";

        public PermissionCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            var cacheKey = $"{CacheKeyPrefix}{roleId}";
            if (_cache.TryGetValue(cacheKey, out List<string>? permissions) && permissions != null)
                return permissions;

            permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Name ?? "")
                .ToListAsync();

            _cache.Set(cacheKey, permissions, TimeSpan.FromHours(1));
            return permissions;
        }

        public void ClearRoleCache(int roleId)
        {
            _cache.Remove($"{CacheKeyPrefix}{roleId}");
        }

        public void ClearAllCache()
        {
            // در IMemoryCache راهی برای حذف گروهی با پیشوند وجود ندارد،
            // پس اینجا کاری نمی‌کنیم و در CacheManager مدیریت خواهد شد
        }
    }
}