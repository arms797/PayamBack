using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.DTOs.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Identity
{
    /// <summary>
    /// کنترلر احراز هویت - ورود، خروج، تمدید توکن و تغییر نقش
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  // همه اکشن‌های این کنترلر بدون احراز هویت قابل دسترس هستند
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ============================================================
        // 1️⃣ ورود کاربر
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Success(response, "ورود موفق");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, 401);
            }
        }

        // ============================================================
        // 2️⃣ تمدید AccessToken با RefreshToken
        // ============================================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Success(response, "توکن بروزرسانی شد");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, 401);
            }
        }

        // ============================================================
        // 3️⃣ خروج کاربر
        // ============================================================
        [HttpPost("logout")]
        [Authorize]  // برای خروج باید احراز هویت شده باشید
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _authService.LogoutAsync(userId);
            return Success( "خروج موفق");
        }

        // ============================================================
        // 4️⃣ تغییر نقش فعال کاربر
        // ============================================================
        [HttpPost("change-role")]
        [Authorize]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var response = await _authService.ChangeRoleAsync(userId, dto.RoleId);
                return Success(response, "نقش با موفقیت تغییر کرد");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, 401);
            }
        }
    }
}