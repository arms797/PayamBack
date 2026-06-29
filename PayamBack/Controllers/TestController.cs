using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PayamBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            // این خطا را در GlobalExceptionMiddleware بررسی می‌کنیم
        http://localhost:5023/api/Test/test-error
            throw new UnauthorizedAccessException("login_invalid");
        }

        [HttpGet("test-captcha")]
        public IActionResult TestCaptcha()
        {
            throw new UnauthorizedAccessException("captcha_invalid");
        }
    }
}