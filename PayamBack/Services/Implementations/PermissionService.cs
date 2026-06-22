using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت دسترسی‌ها و مجوزها
    /// دسترسی‌ها مستقیماً از دیتابیس خوانده می‌شوند (بدون Cache)
    /// چون نقش‌ها تندتند تغییر نمی‌کنند
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ بررسی دسترسی کاربر به یک منبع و عملیات خاص
        // پشتیبانی از "*" برای دسترسی به همه عملیات‌ها
        // ============================================================
        public async Task<bool> HasPermissionAsync(int userId, int roleId, string resource, string action)
        {
            // 1️⃣ بررسی دسترسی "*" (همه عملیات‌ها)
            var hasWildcardPermission = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p)
                .AnyAsync(p => p.Resource == resource && p.Action == "*");

            if (hasWildcardPermission)
                return true;

            // 2️⃣ بررسی دسترسی دقیق
            var permissionName = $"{resource}.{action}";

            var hasExactPermission = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p)
                .AnyAsync(p => p.Name == permissionName);

            return hasExactPermission;
        }

        // ============================================================
        // 2️⃣ گرفتن منوهایی که کاربر بر اساس نقش فعال به آنها دسترسی دارد
        // ============================================================
        public async Task<List<MenuDto>> GetUserMenusAsync(int userId, int roleId)
        {
            // 1️⃣ گرفتن همه مجوزهای نقش فعال
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Name ?? "")
                .ToListAsync();

            // 2️⃣ گرفتن همه منوهای فعال
            var allMenus = await _context.Menus
                .Where(m => m.Vazeeat == true)
                .OrderBy(m => m.Order)
                .ToListAsync();

            // 3️⃣ فیلتر کردن منوها بر اساس مجوزهای نقش
            var accessibleMenus = allMenus
                .Where(m => string.IsNullOrEmpty(m.PermissionName) || rolePermissions.Contains(m.PermissionName))
                .ToList();

            // 4️⃣ تبدیل به ساختار درختی
            var menuDtos = accessibleMenus
                .Where(m => m.ParentId == null)
                .Select(m => MapToMenuDto(m, accessibleMenus))
                .ToList();

            return menuDtos;
        }

        // ============================================================
        // 3️⃣ گرفتن همه نقش‌های کاربر با مشخص کردن نقش پیش‌فرض
        // ============================================================
        public async Task<List<RoleDto>> GetUserRolesAsync(int userId)
        {
            // استفاده از AppUserRole (نه IdentityUserRole)
            var userRoles = await _context.Set<AppUserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? "",
                        IsDefault = ur.RolePishFarz ?? false
                    })
                .ToListAsync();

            return userRoles;
        }

        // ============================================================
        // 4️⃣ گرفتن نقش پیش‌فرض کاربر
        // ============================================================
        public async Task<int?> GetDefaultRoleIdAsync(int userId)
        {
            var userRoles = await GetUserRolesAsync(userId);
            return userRoles.FirstOrDefault(r => r.IsDefault)?.Id;
        }

        // ============================================================
        // متد کمکی برای تبدیل Menu به MenuDto با ساختار درختی
        // ============================================================
        private MenuDto MapToMenuDto(Menu menu, List<Menu> allMenus)
        {
            return new MenuDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Title = menu.Title ?? "",
                Icon = menu.Icon,
                Path = menu.Path,
                PermissionName = menu.PermissionName,
                Order = menu.Order,
                Children = allMenus
                    .Where(m => m.ParentId == menu.Id)
                    .Select(m => MapToMenuDto(m, allMenus))
                    .ToList()
            };
        }
    }
}