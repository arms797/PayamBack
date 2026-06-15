using PayamBack.Dtos;
using PayamBack.Middlewares;
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // لاگ کردن خطا
            _logger.LogError(exception, "خطا در پردازش درخواست: {Message}", exception.Message);

            // تنظیم پاسخ
            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = exception switch
            {
                UnauthorizedAccessException => ApiResponse.Error("شما مجوز دسترسی ندارید", (int)HttpStatusCode.Unauthorized),
                KeyNotFoundException => ApiResponse.Error("اطلاعات درخواستی یافت نشد", (int)HttpStatusCode.NotFound),
                ArgumentException => ApiResponse.Error(exception.Message, (int)HttpStatusCode.BadRequest),
                InvalidOperationException => ApiResponse.Error(exception.Message, (int)HttpStatusCode.Conflict),
                _ => ApiResponse.Error("خطای داخلی سرور. لطفاً با پشتیبانی تماس بگیرید.", (int)HttpStatusCode.InternalServerError)
            };

            response.StatusCode = apiResponse.StatusCode;
            await response.WriteAsync(JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    }
}

// Extension method برای راحت اضافه کردن میدلور
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}