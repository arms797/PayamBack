// PayamBack/Controllers/FileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.Filters;

namespace PayamBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ← کاربر باید لاگین باشد
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // دانلود فایل با مسیر (عمومی و بدون نیاز به مجوز خاص)
        // ============================================================
        [HttpGet("download")]
        [NoPermission]
        public async Task<IActionResult> Download([FromQuery] string path, [FromQuery] string? fileName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return BadRequest(new { message = "مسیر فایل مشخص نشده است" });

                // جلوگیری از دسترسی به فایل‌های خارج از پوشه wwwroot (امنیت)
                var normalizedPath = path.Replace("..", "").Replace("//", "/").TrimStart('/');
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", normalizedPath);

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound(new { message = "فایل در سرور یافت نشد" });

                var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                var originalFileName = Path.GetFileName(physicalPath);
                var originalExtension = Path.GetExtension(originalFileName);

                // ============================================================
                // 🔥 اصلاح نام فایل: اگر fileName داده شده ولی پسوند ندارد، پسوند را اضافه کن
                // ============================================================
                string finalFileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    // اگر fileName پسوند ندارد، پسوند فایل اصلی را اضافه کن
                    if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                    {
                        finalFileName = fileName + originalExtension;
                    }
                    else
                    {
                        finalFileName = fileName;
                    }
                }
                else
                {
                    finalFileName = originalFileName;
                }

                // تشخیص نوع فایل
                var contentType = "application/octet-stream";
                var extension = Path.GetExtension(finalFileName).ToLower();
                if (extension == ".pdf") contentType = "application/pdf";
                else if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";
                else if (extension == ".png") contentType = "image/png";
                else if (extension == ".docx") contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                else if (extension == ".xlsx") contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, finalFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دانلود فایل", error = ex.Message });
            }
        }
    }
}