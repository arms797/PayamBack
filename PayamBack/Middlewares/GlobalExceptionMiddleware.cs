using System.Net;
using System.Text.Json;

namespace PayamBack.Middlewares
{
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
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش درخواست: {Message}", ex.Message);

                var (statusCode, message) = ex switch
                {
                    UnauthorizedAccessException => ex.Message switch
                    {
                        "captcha_required" => (HttpStatusCode.BadRequest, "لطفاً کد امنیتی را وارد کنید"),
                        "captcha_invalid" => (HttpStatusCode.BadRequest, "کد امنیتی اشتباه است"),
                        "login_invalid" => (HttpStatusCode.Unauthorized, "نام کاربری یا رمز عبور اشتباه است"),
                        _ => (HttpStatusCode.Unauthorized, "شما مجوز دسترسی ندارید")
                    },
                    KeyNotFoundException => (HttpStatusCode.NotFound, "اطلاعات درخواستی یافت نشد"),
                    ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                    InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "خطای داخلی سرور. لطفاً با پشتیبانی تماس بگیرید.")
                };

                var response = new
                {
                    success = false,
                    message = message,
                    statusCode = (int)statusCode
                };

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

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}