using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.Models.Captcha;
using PayamBack.Services.Interfaces;

namespace PayamBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  // بدون احراز هویت
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captchaService;

        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        // ============================================================
        // 1️⃣ دریافت CAPTCHA جدید
        // ============================================================
        [HttpGet("generate")]
        public IActionResult Generate()
        {
            try
            {
                var captcha = _captchaService.GenerateCaptcha();
                return Ok(new
                {
                    success = true,
                    message = "CAPTCHA ساخته شد",
                    data = captcha
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تولید کد امنیتی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ اعتبارسنجی CAPTCHA
        // ============================================================
        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CaptchaValidationRequest request)
        {
            try
            {
                // اعتبارسنجی ورودی
                if (string.IsNullOrEmpty(request.CaptchaKey) || string.IsNullOrEmpty(request.UserAnswer))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "اطلاعات کد امنیتی کامل نیست"
                    });
                }

                var isValid = _captchaService.ValidateCaptcha(request.CaptchaKey, request.UserAnswer);

                if (!isValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "کد امنیتی اشتباه است"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "کد امنیتی صحیح است",
                    data = new { valid = true }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در اعتبارسنجی کد امنیتی",
                    error = ex.Message
                });
            }
        }
    }
}