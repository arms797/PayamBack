using PayamBack.Models.Identity;
using System.Security.Claims;

namespace PayamBack.Services.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت توکن‌ها
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// ساخت AccessToken (JWT) برای کاربر
        /// </summary>
        Task<string> GenerateAccessToken(AppUser user);

        /// <summary>
        /// ساخت RefreshToken و ذخیره در Identity (جدول AspNetUserTokens) + Cache
        /// </summary>
        Task<string> GenerateRefreshToken(AppUser user);

        /// <summary>
        /// بررسی اعتبار RefreshToken (از Cache یا Identity)
        /// </summary>
        Task<bool> ValidateRefreshToken(AppUser user, string refreshToken);

        /// <summary>
        /// باطل کردن RefreshToken (حذف از Cache و Identity)
        /// </summary>
        Task RevokeRefreshToken(AppUser user);

        /// <summary>
        /// خواندن اطلاعات از یک AccessToken منقضی شده (برای تمدید)
        /// </summary>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}