using PayamBack.DTOs.Identity;

namespace PayamBack.Services.Interfaces
{
    public interface IPermissionService
    {
        // دریافت مجوزهای یک نقش (برای لاگین و تغییر نقش)
        Task<List<string>> GetRolePermissionsAsync(int roleId);

        // بررسی دسترسی با لیست مجوزها (بدون کوئری)
        bool HasPermission(List<string> permissions, string resource, string action);

        // گرفتن منوهای کاربر (برای لاگین و تغییر نقش)
        Task<List<MenuDto>> GetUserMenusAsync(int userId, int roleId, List<string> permissions);

        // گرفتن نقش‌های کاربر (برای لاگین)
        Task<List<RoleDto>> GetUserRolesAsync(int userId);

        // گرفتن نقش پیش‌فرض کاربر (برای لاگین)
        Task<int?> GetDefaultRoleIdAsync(int userId);
    }
}