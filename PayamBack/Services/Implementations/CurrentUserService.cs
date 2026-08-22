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

        public async Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync()
        {
            // 1️⃣ دریافت UserId از JWT
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (null, null, null, null);

            var cacheKey = $"{CacheKeyPrefix}{userId}";

            // 2️⃣ بررسی کش
            if (_cache.TryGetValue(cacheKey, out CachedUserInfo cachedInfo))
            {
                return (cachedInfo.User, cachedInfo.Role, cachedInfo.Markaz, cachedInfo.CodeRole);
            }

            // 3️⃣ دریافت از دیتابیس
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, null, null, null);

            // دریافت نقش فعال از JWT
            var roleName = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value;

            AppRole? role = null;
            Markaz? markaz = null;
            int codeRole = 4; // پیش‌فرض: مرکز

            if (!string.IsNullOrEmpty(roleName))
            {
                role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // دریافت AppUserRole برای این کاربر و نقش
                    var activeRole = await _context.Set<AppUserRole>()
                        .Where(ur => ur.UserId == user.Id && ur.RoleId == role.Id && ur.RolePishFarz == true)
                        .FirstOrDefaultAsync();

                    if (activeRole?.MarkazId != null)
                    {
                        markaz = await _context.Markazes.FindAsync(activeRole.MarkazId.Value);
                    }
                    codeRole = role.CodeRole ?? 4;
                }
            }

            // 4️⃣ ذخیره در کش (زمان انقضا: ۱۰ دقیقه)
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