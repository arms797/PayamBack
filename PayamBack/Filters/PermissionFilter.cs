using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using System.Security.Claims;

namespace PayamBack.Filters
{
    public class PermissionFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PermissionFilter(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ============================================================
            // 1️⃣ اگر اکشن یا کنترلر دارای AllowAnonymous است، از بررسی مجوز صرف‌نظر کن
            // ============================================================
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;

            var controllerAllowAnonymous = context.Controller.GetType()
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .Any();

            if (allowAnonymous || controllerAllowAnonymous)
            {
                await next();
                return;
            }

            // ============================================================
            // 2️⃣ بررسی احراز هویت
            // ============================================================
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                await next();
                return;
            }

            // ============================================================
            // 3️⃣ دریافت نقش فعال کاربر
            // ============================================================
            var activeRoleId = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.RolePishFarz == true)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            if (activeRoleId == 0)
            {
                context.Result = new ForbidResult();
                return;
            }

            // ============================================================
            // 4️⃣ دریافت نام کنترلر و اکشن
            // ============================================================
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
            var actionName = context.ActionDescriptor.RouteValues["action"] ?? "";
            var permissionName = $"{controllerName}.{actionName}";

            // ============================================================
            // 5️⃣ بررسی مجوز از Cache
            // ============================================================
            var cacheKey = $"Permission_{activeRoleId}_{permissionName}";
            var hasPermission = _cache.Get<bool?>(cacheKey);

            if (!hasPermission.HasValue)
            {
                // ابتدا بررسی کن که آیا نقش به "*" دسترسی دارد
                var wildcardPermissionName = $"{controllerName}.*";

                var hasWildcardPermission = await _context.RolePermissions
                    .Where(rp => rp.RoleId == activeRoleId && rp.Vazeeat == true)
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p.Name == wildcardPermissionName)
                    .AnyAsync();

                if (hasWildcardPermission)
                {
                    hasPermission = true;
                }
                else
                {
                    hasPermission = await _context.RolePermissions
                        .Where(rp => rp.RoleId == activeRoleId && rp.Vazeeat == true)
                        .Join(_context.Permissions,
                            rp => rp.PermissionId,
                            p => p.Id,
                            (rp, p) => p.Name == permissionName)
                        .AnyAsync();
                }

                _cache.Set(cacheKey, hasPermission.Value, TimeSpan.FromMinutes(10));
            }

            if (!hasPermission.Value)
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = "شما مجوز دسترسی به این بخش را ندارید"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}