// PayamBack/Services/Implementations/AccessService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Services.Implementations
{
    public class AccessService : IAccessService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMarkazCacheService _markazCache;

        public AccessService(
            AppDbContext context,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService,
            IMarkazCacheService markazCache)
        {
            _context = context;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
            _markazCache = markazCache;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        public async Task<bool> IsOstadUserAsync(int userId)
        {
            if (userId == GetCurrentUserId())
            {
                var (user, _, _, _) = await _currentUserService.GetCurrentUserInfoAsync();
                return user?.OstadId != null;
            }

            var cacheKey = $"IsOstad_{userId}";
            if (_cache.TryGetValue(cacheKey, out bool isOstad))
                return isOstad;

            var targetUser = await _context.Users.FindAsync(userId);
            isOstad = targetUser?.OstadId != null;
            _cache.Set(cacheKey, isOstad, TimeSpan.FromMinutes(5));
            return isOstad;
        }

        public async Task<bool> CanAccessTargetUserAsync(int targetUserId, int codeRole, int? currentMarkazId)
        {
            if (targetUserId == GetCurrentUserId())
                return true;

            var targetUser = await _context.Users
                .Include(u => u.Ostad)
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null) return false;

            if (targetUser.Ostad?.MarkazId != null)
            {
                return await CanAccessTargetOstadAsync(
                    targetUser.Ostad.Id, // ← ارسال OstadId
                    codeRole,
                    currentMarkazId);
            }

            return false;
        }

        public async Task<bool> CanAccessTargetMarkazAsync(int targetMarkazId, int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1 || codeRole == 2) return true;

            var allMarkaz = await _markazCache.GetAllAsync();
            var targetMarkaz = allMarkaz.FirstOrDefault(m => m.Id == targetMarkazId);
            var currentMarkaz = allMarkaz.FirstOrDefault(m => m.Id == currentMarkazId);

            if (targetMarkaz == null || currentMarkaz == null) return false;

            if (codeRole == 3)
                return targetMarkaz.CodeOstan == currentMarkaz.CodeOstan;

            if (codeRole == 4)
                return targetMarkaz.Id == currentMarkaz.Id;

            return false;
        }

        public async Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId)
        {
            var allMarkaz = await _markazCache.GetAllAsync();

            if (codeRole == 1 || codeRole == 2)
                return allMarkaz.Select(m => m.Id).ToList();

            var currentMarkaz = allMarkaz.FirstOrDefault(m => m.Id == currentMarkazId);
            if (currentMarkaz == null) return new List<int>();

            if (codeRole == 3)
                return allMarkaz
                    .Where(m => m.CodeOstan == currentMarkaz.CodeOstan)
                    .Select(m => m.Id)
                    .ToList();

            if (codeRole == 4)
                return new List<int> { currentMarkaz.Id };

            return new List<int>();
        }
        // PayamBack/Services/Implementations/AccessService.cs
        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            var cacheKey = $"RoleId_{roleName}";
            if (_cache.TryGetValue(cacheKey, out int roleId))
                return roleId;

            var role = await _context.Roles
                .Where(r => r.Name == roleName)
                .Select(r => new { r.Id })
                .FirstOrDefaultAsync();

            if (role == null) return null;

            _cache.Set(cacheKey, role.Id, TimeSpan.FromDays(1));
            return role.Id;
        }
        // PayamBack/Services/Implementations/AccessService.cs
        public async Task<bool> CanAccessTargetOstadAsync(int ostadId, int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1 || codeRole == 2) return true;

            // دریافت لیست مراکز از کش
            var allMarkaz = await _markazCache.GetAllAsync();

            // دریافت اطلاعات استاد
            var ostad = await _context.Ostads
                .Include(o => o.Markaz)
                .FirstOrDefaultAsync(o => o.Id == ostadId);

            if (ostad == null || ostad.MarkazId == null) return false;

            // پیدا کردن مرکز هدف و مرکز فعلی از لیست کش‌شده
            var targetMarkaz = allMarkaz.FirstOrDefault(m => m.Id == ostad.MarkazId.Value);
            var currentMarkaz = allMarkaz.FirstOrDefault(m => m.Id == currentMarkazId);

            if (targetMarkaz == null || currentMarkaz == null) return false;

            if (codeRole == 3)
                return targetMarkaz.CodeOstan == currentMarkaz.CodeOstan;

            if (codeRole == 4)
                return targetMarkaz.Id == currentMarkaz.Id;

            return false;
        }
    }
}