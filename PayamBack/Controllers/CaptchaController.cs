using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.Models.Captcha;
using PayamBack.Services.Interfaces;

namespace PayamBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  // بدون احراز هویت
    public class CaptchaController : BaseController
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
            var captcha = _captchaService.GenerateCaptcha();
            return Success(captcha, "CAPTCHA ساخته شد");
        }

        // ============================================================
        // 2️⃣ اعتبارسنجی CAPTCHA
        // ============================================================
        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CaptchaValidationRequest request)
        {
            var isValid = _captchaService.ValidateCaptcha(request.CaptchaKey, request.UserAnswer);

            if (!isValid)
                return Error("کد امنیتی اشتباه است", 400);

            return Success(new { valid = true }, "کد امنیتی صحیح است");
        }
    }
}