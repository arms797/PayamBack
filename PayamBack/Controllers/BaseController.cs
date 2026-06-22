using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers
{
    /// <summary>
    /// کنترلر پایه با مدیریت خودکار دسترسی‌ها
    /// پیاده‌سازی IAsyncActionFilter برای بررسی دسترسی قبل از هر اکشن
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase, IAsyncActionFilter
    {
        /// <summary>
        /// متد بررسی دسترسی قبل از هر اکشن
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                // اگر کاربر احراز هویت نشده، ادامه بده (دسترسی عمومی)
                if (!User.Identity?.IsAuthenticated == true)
                {
                    await next();
                    return;
                }

                // گرفتن سرویس دسترسی از DI
                var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
                if (permissionService == null)
                {
                    await next();
                    return;
                }

                // گرفتن شناسه کاربر از Claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    await next();
                    return;
                }

                // گرفتن نقش فعال کاربر
                var roleId = await permissionService.GetDefaultRoleIdAsync(userId);
                if (!roleId.HasValue)
                {
                    await next();
                    return;
                }

                // گرفتن نام کنترلر و اکشن
                var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
                var actionName = context.ActionDescriptor.RouteValues["action"] ?? "";

                // بررسی دسترسی
                var hasPermission = await permissionService.HasPermissionAsync(userId, roleId.Value, controllerName, actionName);
                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                    return;
                }

                // اگر دسترسی داشت، ادامه بده
                await next();
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                var logger = context.HttpContext.RequestServices.GetService<ILogger<BaseController>>();
                logger?.LogError(ex, "خطا در بررسی دسترسی");

                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = "خطای داخلی سرور. لطفاً با پشتیبانی تماس بگیرید."
                })
                {
                    StatusCode = 500
                };
            }
        }

        // ============================================================
        // متدهای کمکی برای پاسخ‌های استاندارد
        // ============================================================

        /// <summary>
        /// پاسخ موفقیت بدون داده (فقط پیام)
        /// </summary>
        protected IActionResult Success(string message = "با موفقیت انجام شد")
        {
            return Ok(new { success = true, message });
        }

        /// <summary>
        /// پاسخ موفقیت با داده
        /// </summary>
        protected IActionResult Success<T>(T data, string message = "با موفقیت انجام شد")
        {
            return Ok(new { success = true, message, data });
        }

        /// <summary>
        /// پاسخ خطا
        /// </summary>
        protected IActionResult Error(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new { success = false, message });
        }

        /// <summary>
        /// پاسخ NotFound
        /// </summary>
        protected IActionResult NotFoundError(string message = "رکورد یافت نشد")
        {
            return NotFound(new { success = false, message });
        }

        /// <summary>
        /// پاسخ خطای سرور
        /// </summary>
        protected IActionResult ServerError(string message = "خطای داخلی سرور")
        {
            return StatusCode(500, new { success = false, message });
        }
    }
}