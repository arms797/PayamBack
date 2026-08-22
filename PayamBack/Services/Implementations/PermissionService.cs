// PayamBack/Services/Implementations/PermissionService.cs
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly IPermissionCacheService _permissionCacheService;

        public PermissionService(AppDbContext context, IPermissionCacheService permissionCacheService)
        {
            _context = context;
            _permissionCacheService = permissionCacheService;
        }

        // ============================================================
        // 1️⃣ دریافت مجوزهای یک نقش (با استفاده از کش)
        // ============================================================
        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            return await _permissionCacheService.GetRolePermissionsAsync(roleId);
        }

        // ============================================================
        // 2️⃣ بررسی دسترسی با لیست مجوزها
        // ============================================================
        public bool HasPermission(List<string> permissions, string resource, string action)
        {
            var normalizedAction = NormalizeAction(action);
            var permissionName = $"{resource}.{normalizedAction}";

            if (permissions.Any(p => p == $"{resource}.*"))
                return true;

            return permissions.Contains(permissionName);
        }

        // ============================================================
        // 3️⃣ گرفتن منوهای کاربر
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
        // 4️⃣ گرفتن همه نقش‌های کاربر
        // ============================================================
        public async Task<List<RoleDto>> GetUserRolesAsync(int userId)
        {
            return await _context.Set<AppUserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles.Where(r => r.Vazeeyat == true),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur, r })
                .Join(_context.Markazes.Where(m => m.Vazeeyat == true),
                    ur => ur.ur.MarkazId,
                    m => m.Id,
                    (ur, m) => new RoleDto
                    {
                        Id = ur.r.Id,
                        Name = ur.r.Name ?? "",
                        IsDefault = ur.ur.RolePishFarz ?? false,
                        MarkazId = ur.ur.MarkazId ?? 0,
                        CodeRole = ur.r.CodeRole ?? 4,
                        IsAdmin = ur.r.IsAdmin ?? false
                    })
                .ToListAsync();
        }

        // ============================================================
        // 5️⃣ گرفتن نقش پیش‌فرض کاربر
        // ============================================================
        public async Task<int?> GetDefaultRoleIdAsync(int userId)
        {
            var userRoles = await GetUserRolesAsync(userId);
            return userRoles.FirstOrDefault(r => r.IsDefault)?.Id;
        }

        // ============================================================
        // متدهای کمکی
        // ============================================================
        private string NormalizeAction(string action)
        {
            if (action.StartsWith("Get") ||
                action == "List" || action == "All" || action == "Active" ||
                action == "Inactive" || action == "Search" || action == "Filter" ||
                action == "Index" || action == "Details")
                return "View";

            if (action == "Create" || action == "Add" || action == "Insert" || action == "Register")
                return "Create";

            if (action == "Update" || action == "Edit" || action == "Modify" ||
                action == "Change" || action == "Toggle" || action == "Active" ||
                action == "Deactive" || action == "Activate" || action == "Deactivate")
                return "Update";

            if (action == "Delete" || action == "Remove" || action == "Deactivate" || action == "Archive")
                return "Delete";

            if (action == "BulkUpload")
                return "BulkUpload";

            return action;
        }

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