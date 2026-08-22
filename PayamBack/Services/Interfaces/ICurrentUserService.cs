// PayamBack/Services/Interfaces/ICurrentUserService.cs
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Services.Interfaces
{
    /// <summary>
    /// سرویس مدیریت اطلاعات کاربر فعلی با کش
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// دریافت اطلاعات کاربر فعلی (با کش)
        /// </summary>
        Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync();

        /// <summary>
        /// پاک کردن کش اطلاعات یک کاربر خاص
        /// </summary>
        void ClearCache(int userId);
    }
}