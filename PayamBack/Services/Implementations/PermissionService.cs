using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت دسترسی‌ها و مجوزها
    /// مجوزها و منوها فقط در زمان لاگین و تغییر نقش از دیتابیس خوانده می‌شوند
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت همه مجوزهای یک نقش (فقط در لاگین و تغییر نقش)
        // ============================================================
        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Name ?? "")
                .ToListAsync();

            return permissions;
        }

        // ============================================================
        // 2️⃣ بررسی دسترسی با لیست مجوزها (بدون کوئری)
        // این متد در PermissionFilter استفاده می‌شود
        // ============================================================
        public bool HasPermission(List<string> permissions, string resource, string action)
        {
            // نرمال‌سازی action به View, Create, Update, Delete
            var normalizedAction = NormalizeAction(action);
            var permissionName = $"{resource}.{normalizedAction}";

            // بررسی دسترسی "*" (همه عملیات‌ها)
            if (permissions.Any(p => p == $"{resource}.*"))
                return true;

            // بررسی دسترسی دقیق
            return permissions.Contains(permissionName);
        }

        // ============================================================
        // 3️⃣ گرفتن منوهای کاربر (فقط در لاگین و تغییر نقش)
        // ============================================================
        public async Task<List<MenuDto>> GetUserMenusAsync(int userId, int roleId, List<string> permissions)
        {
            var allMenus = await _context.Menus
                .Where(m => m.Vazeeat == true)
                .OrderBy(m => m.Order)
                .ToListAsync();

            var accessibleMenus = allMenus
                .Where(m => string.IsNullOrEmpty(m.PermissionName) || permissions.Contains(m.PermissionName))
                .ToList();

            return accessibleMenus
                .Where(m => m.ParentId == null)
                .Select(m => MapToMenuDto(m, accessibleMenus))
                .ToList();
        }

        // ============================================================
        // 4️⃣ گرفتن همه نقش‌های کاربر (فقط در لاگین)
        // ============================================================
        public async Task<List<RoleDto>> GetUserRolesAsync(int userId)
        {
            return await _context.Set<AppUserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? "",
                        IsDefault = ur.RolePishFarz ?? false,
                        MarkazId = ur.MarkazId ?? 0
                    })
                .ToListAsync();
        }

        // ============================================================
        // 5️⃣ گرفتن نقش پیش‌فرض کاربر (فقط در لاگین)
        // ============================================================
        public async Task<int?> GetDefaultRoleIdAsync(int userId)
        {
            var userRoles = await GetUserRolesAsync(userId);
            return userRoles.FirstOrDefault(r => r.IsDefault)?.Id;
        }

        // ============================================================
        // متد کمکی برای تبدیل اکشن‌ها به چهار نوع اصلی
        // ============================================================
        private string NormalizeAction(string action)
        {
            // 1️⃣ خواندن → View
            if (action.StartsWith("Get") ||
                action == "List" || action == "All" || action == "Active" ||
                action == "Inactive" || action == "Search" || action == "Filter" ||
                action == "Index" || action == "Details")
                return "View";

            // 2️⃣ ایجاد → Create
            if (action == "Create" || action == "Add" || action == "Insert" || action == "Register")
                return "Create";

            // 3️⃣ ویرایش → Update
            if (action == "Update" || action == "Edit" || action == "Modify" ||
                action == "Change" || action == "Toggle" || action == "Active" ||
                action == "Deactive" || action == "Activate" || action == "Deactivate")
                return "Update";

            // 4️⃣ حذف → Delete
            if (action == "Delete" || action == "Remove" || action == "Deactivate" || action == "Archive")
                return "Delete";

            // 5️⃣ BulkUpload → مجوز خاص (فقط ادمین)
            if (action == "BulkUpload")
                return "BulkUpload";

            return action;
        }

        // ============================================================
        // متد کمکی برای ساخت منوی درختی
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