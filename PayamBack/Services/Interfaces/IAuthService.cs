using PayamBack.DTOs.Identity;

namespace PayamBack.Services.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس احراز هویت
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// ورود کاربر - تولید AccessToken و RefreshToken
        /// </summary>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// تمدید AccessToken با RefreshToken
        /// </summary>
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// خروج کاربر - باطل کردن RefreshToken
        /// </summary>
        Task<bool> LogoutAsync(int userId);

        /// <summary>
        /// تغییر نقش فعال کاربر - بروزرسانی منوها و دسترسی‌ها
        /// </summary>
        Task<LoginResponseDto> ChangeRoleAsync(int userId, int newRoleId,int?markazId);
    }
}