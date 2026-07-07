using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.DTOs.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ============================================================
        // 1️⃣ ورود
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "ورود موفق",
                    data = response
                });
            }
            catch (Exception ex)
            {
                // ============================================================
                // مدیریت مستقیم خطاها بدون رفتن به جای دیگر
                // ============================================================
                return ex.Message switch
                {
                    "captcha_required" => BadRequest(new { success = false, message = "لطفاً کد امنیتی را وارد کنید" }),
                    "captcha_invalid" => BadRequest(new { success = false, message = "کد امنیتی اشتباه است" }),
                    "login_invalid" => BadRequest(new { success = false, message = "نام کاربری یا رمز عبور اشتباه است" }),
                    _ => StatusCode(500, new { success = false, message = "خطای داخلی سرور" })
                };
            }
        }

        // ============================================================
        // 2️⃣ تمدید توکن
        // ============================================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "توکن بروزرسانی شد",
                    data = response
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ خروج
        // ============================================================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _authService.LogoutAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "خروج موفق"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطای داخلی سرور"
                });
            }
        }

        // ============================================================
        // 4️⃣ تغییر نقش
        // ============================================================
        [HttpPost("change-role")]
        [Authorize]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var response = await _authService.ChangeRoleAsync(userId, dto.RoleId);
                return Ok(new
                {
                    success = true,
                    message = "نقش با موفقیت تغییر کرد",
                    data = response
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}