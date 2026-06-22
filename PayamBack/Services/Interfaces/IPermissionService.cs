using PayamBack.DTOs.Identity;

namespace PayamBack.Services.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت دسترسی‌ها و مجوزها
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// بررسی دسترسی کاربر به یک منبع و عملیات خاص
        /// پشتیبانی از "*" برای دسترسی به همه عملیات‌ها
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, int roleId, string resource, string action);

        /// <summary>
        /// گرفتن منوهایی که کاربر بر اساس نقش فعال به آنها دسترسی دارد
        /// </summary>
        Task<List<MenuDto>> GetUserMenusAsync(int userId, int roleId);

        /// <summary>
        /// گرفتن همه نقش‌های کاربر با مشخص کردن نقش پیش‌فرض
        /// </summary>
        Task<List<RoleDto>> GetUserRolesAsync(int userId);

        /// <summary>
        /// گرفتن نقش پیش‌فرض کاربر
        /// </summary>
        Task<int?> GetDefaultRoleIdAsync(int userId);
    }
}