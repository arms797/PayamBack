// PayamBack/Services/Implementations/CurrentUserService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string CacheKeyPrefix = "UserInfo_";

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            AppDbContext context,
            IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _cache = cache;
        }

        /*
        public async Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync()
        {
            // 1️⃣ دریافت UserId از JWT
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (null, null, null, null);

            var cacheKey = $"{CacheKeyPrefix}{userId}";

            // 2️⃣ بررسی کش (فقط برای User)
            if (_cache.TryGetValue(cacheKey, out AppUser? cachedUser) && cachedUser != null)
            {
                // اطلاعات نقش و مرکز را مستقیم از Claimها می‌خوانیم
                var role = await GetRoleFromClaimsAsync();
                var markaz = await GetMarkazFromClaimsAsync();
                var codeRole = GetCodeRoleFromClaims();

                return (cachedUser, role, markaz, codeRole);
            }

            // 3️⃣ دریافت کاربر از دیتابیس (اگر در کش نبود)
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, null, null, null);

            // ذخیره کاربر در کش (بدون اطلاعات نقش و مرکز)
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));

            // 4️⃣ خواندن نقش و مرکز از Claimها
            var roleFromClaim = await GetRoleFromClaimsAsync();
            var markazFromClaim = await GetMarkazFromClaimsAsync();
            var codeRoleFromClaim = GetCodeRoleFromClaims();

            return (user, roleFromClaim, markazFromClaim, codeRoleFromClaim);
        }

        // ============================================================
        // 🔥 متدهای کمکی برای خواندن از Claimها
        // ============================================================

        private async Task<AppRole?> GetRoleFromClaimsAsync()
        {
            var roleIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("RoleId")?.Value;

            if (string.IsNullOrEmpty(roleIdClaim) || !int.TryParse(roleIdClaim, out int roleId))
                return null;

            // از کش نقش استفاده کنید تا کوئری اضافی نزنید
            var cacheKey = $"Role_{roleId}";
            if (_cache.TryGetValue(cacheKey, out AppRole? cachedRole) && cachedRole != null)
                return cachedRole;

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role != null)
            {
                _cache.Set(cacheKey, role, TimeSpan.FromHours(1));
            }
            return role;
        }

        private async Task<Markaz?> GetMarkazFromClaimsAsync()
        {
            var markazIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("MarkazId")?.Value;

            if (string.IsNullOrEmpty(markazIdClaim) || !int.TryParse(markazIdClaim, out int markazId))
                return null;

            // از کش مرکز استفاده کنید
            var cacheKey = $"Markaz_{markazId}";
            if (_cache.TryGetValue(cacheKey, out Markaz? cachedMarkaz) && cachedMarkaz != null)
                return cachedMarkaz;

            var markaz = await _context.Markazes.FindAsync(markazId);
            if (markaz != null)
            {
                _cache.Set(cacheKey, markaz, TimeSpan.FromHours(6));
            }
            return markaz;
        }

        private int GetCodeRoleFromClaims()
        {
            var codeRoleClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("CodeRole")?.Value;

            if (string.IsNullOrEmpty(codeRoleClaim) || !int.TryParse(codeRoleClaim, out int codeRole))
                return 4; // پیش‌فرض

            return codeRole;
        }

        */

        public async Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync()
        {
            // 1️⃣ دریافت UserId از JWT
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (null, null, null, null);

            var cacheKey = $"{CacheKeyPrefix}{userId}";

            // 2️⃣ بررسی کش (اطلاعات کامل)
            if (_cache.TryGetValue(cacheKey, out CachedUserInfo cachedInfo))
            {
                return (cachedInfo.User, cachedInfo.Role, cachedInfo.Markaz, cachedInfo.CodeRole);
            }

            // 3️⃣ خواندن از Claimها (بدون کوئری دیتابیس)
            var roleIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("RoleId")?.Value;
            var markazIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("MarkazId")?.Value;
            var codeRoleClaim = _httpContextAccessor.HttpContext?.User.FindFirst("CodeRole")?.Value;
            var roleName = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

            AppRole? role = null;
            Markaz? markaz = null;
            int codeRole = 4;

            // 4️⃣ دریافت نقش از کش یا دیتابیس (با RoleId)
            if (!string.IsNullOrEmpty(roleIdClaim) && int.TryParse(roleIdClaim, out int roleId))
            {
                var roleCacheKey = $"Role_{roleId}";
                if (!_cache.TryGetValue(roleCacheKey, out role) || role == null)
                {
                    role = await _roleManager.FindByIdAsync(roleId.ToString());
                    if (role != null)
                        _cache.Set(roleCacheKey, role, TimeSpan.FromHours(1));
                }
            }

            // اگر نقش از RoleId پیدا نشد، از RoleName استفاده کن (فال‌بک)
            if (role == null && !string.IsNullOrEmpty(roleName))
            {
                role = await _roleManager.FindByNameAsync(roleName);
            }

            // 5️⃣ دریافت مرکز از کش یا دیتابیس (با MarkazId)
            if (!string.IsNullOrEmpty(markazIdClaim) && int.TryParse(markazIdClaim, out int markazId))
            {
                var markazCacheKey = $"Markaz_{markazId}";
                if (!_cache.TryGetValue(markazCacheKey, out markaz) || markaz == null)
                {
                    markaz = await _context.Markazes.FindAsync(markazId);
                    if (markaz != null)
                        _cache.Set(markazCacheKey, markaz, TimeSpan.FromHours(6));
                }
            }

            // 6️⃣ دریافت CodeRole
            if (!string.IsNullOrEmpty(codeRoleClaim) && int.TryParse(codeRoleClaim, out int parsedCodeRole))
            {
                codeRole = parsedCodeRole;
            }
            else if (role != null)
            {
                codeRole = role.CodeRole ?? 4;
            }

            // 7️⃣ دریافت کاربر (از دیتابیس)
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, null, null, null);

            // 8️⃣ ذخیره در کش (اطلاعات کامل)
            var cached = new CachedUserInfo
            {
                User = user,
                Role = role,
                Markaz = markaz,
                CodeRole = codeRole
            };

            _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(10));

            return (user, role, markaz, codeRole);
        }
        public void ClearCache(int userId)
        {
            var cacheKey = $"{CacheKeyPrefix}{userId}";
            _cache.Remove(cacheKey);
        }
    }
}