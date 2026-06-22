using System.Net;
using System.Text.Json;

namespace PayamBack.Middlewares
{
    /// <summary>
    /// میدلور مدیریت سراسری خطاها
    /// با گرفتن همه خطاها، از کرش کردن برنامه جلوگیری می‌کند
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // اجرای درخواست
                await _next(context);
            }
            catch (Exception ex)
            {
                // ============================================================
                // مدیریت خطا: لاگ کردن و برگرداندن پاسخ مناسب
                // ============================================================

                // 1️⃣ لاگ کردن خطا
                _logger.LogError(ex, "خطا در پردازش درخواست: {Message}", ex.Message);

                // 2️⃣ تشخیص نوع خطا و تنظیم کد وضعیت مناسب
                var (statusCode, message) = ex switch
                {
                    UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "شما مجوز دسترسی ندارید"),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "اطلاعات درخواستی یافت نشد"),
                    ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                    InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "خطای داخلی سرور. لطفاً با پشتیبانی تماس بگیرید.")
                };

                // 3️⃣ ساخت پاسخ استاندارد
                var response = new
                {
                    success = false,
                    message = message,
                    statusCode = (int)statusCode
                };

                // 4️⃣ برگرداندن پاسخ به کاربر (بدون کرش)
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        }
    }

    // ============================================================
    // متد توسعه برای ثبت میدلور در Program.cs
    // ============================================================
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}