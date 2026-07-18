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
            // 1️⃣ بررسی AllowAnonymous
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

            // 2️⃣ بررسی احراز هویت
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                await next();
                return;
            }

            // 3️⃣ دریافت اطلاعات از JWT
            var markazIdClaim = context.HttpContext.User.FindFirst("MarkazId")?.Value;
            var codeRoleClaim = context.HttpContext.User.FindFirst("CodeRole")?.Value;
            var ostanIdClaim = context.HttpContext.User.FindFirst("OstanId")?.Value;
            var roleClaims = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            int? userMarkazId = string.IsNullOrEmpty(markazIdClaim) ? null : int.Parse(markazIdClaim);
            int? userCodeRole = string.IsNullOrEmpty(codeRoleClaim) ? 4 : int.Parse(codeRoleClaim);
            string? userOstanId = string.IsNullOrEmpty(ostanIdClaim) ? null : ostanIdClaim;

            // 4️⃣ دریافت نقش فعال از JWT
            var activeRoleName = roleClaims.FirstOrDefault();
            if (string.IsNullOrEmpty(activeRoleName))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 5️⃣ دریافت RoleId از Cache
            var roleCacheKey = $"RoleId_{activeRoleName}";
            var activeRoleId = _cache.Get<int?>(roleCacheKey);

            if (!activeRoleId.HasValue)
            {
                // ✅ استفاده از FirstOrDefaultAsync با نوع مشخص
                activeRoleId = await _context.Roles
                    .Where(r => r.Name == activeRoleName)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync<int>();  // ← صریحاً نوع مشخص شده

                if (activeRoleId == 0)
                {
                    context.Result = new ForbidResult();
                    return;
                }

                _cache.Set(roleCacheKey, activeRoleId.Value, TimeSpan.FromDays(1));
            }

            // 6️⃣ دریافت نام کنترلر و اکشن
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
            var actionName = context.ActionDescriptor.RouteValues["action"] ?? "";
            var permissionName = $"{controllerName}.{actionName}";

            // 7️⃣ بررسی مجوز از Cache
            var permissionCacheKey = $"Permission_{activeRoleId.Value}_{permissionName}";
            var hasPermission = _cache.Get<bool?>(permissionCacheKey);

            if (!hasPermission.HasValue)
            {
                var wildcardPermissionName = $"{controllerName}.*";

                // ✅ استفاده از AnyAsync با نوع مشخص
                var hasWildcardPermission = await _context.RolePermissions
                    .Where(rp => rp.RoleId == activeRoleId.Value && rp.Vazeeat == true)
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p)
                    .Where(p => p.Name == wildcardPermissionName)
                    .AnyAsync();  // ← AnyAsync بدون نیاز به نوع مشخص

                if (hasWildcardPermission)
                {
                    hasPermission = true;
                }
                else
                {
                    hasPermission = await _context.RolePermissions
                        .Where(rp => rp.RoleId == activeRoleId.Value && rp.Vazeeat == true)
                        .Join(_context.Permissions,
                            rp => rp.PermissionId,
                            p => p.Id,
                            (rp, p) => p)
                        .Where(p => p.Name == permissionName)
                        .AnyAsync();  // ← AnyAsync بدون نیاز به نوع مشخص
                }

                _cache.Set(permissionCacheKey, hasPermission.Value, TimeSpan.FromMinutes(10));
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

            // 8️⃣ بررسی سطح دسترسی
            var isChangeAction = actionName == "Create" ||
                                 actionName == "Update" ||
                                 actionName == "Delete" ||
                                 actionName == "BulkUpload";

            if (isChangeAction)
            {
                var targetMarkazId = GetTargetMarkazId(context);

                var hasAccessLevel = CheckAccessLevel(
                    userMarkazId,
                    userCodeRole,
                    userOstanId,
                    targetMarkazId);

                if (!hasAccessLevel)
                {
                    context.Result = new ObjectResult(new
                    {
                        success = false,
                        message = "شما مجوز تغییر این داده را ندارید"
                    })
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            await next();
        }

        private int? GetTargetMarkazId(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("markazId", out var value) && value is int id)
                return id;

            if (context.ActionArguments.TryGetValue("id", out var idValue) && idValue is int idInt)
            {
                var controllerName = context.Controller.GetType().Name;
                if (controllerName.Contains("Markaz"))
                    return idInt;
            }

            return null;
        }

        private bool CheckAccessLevel(
            int? userMarkazId,
            int? userCodeRole,
            string? userOstanId,
            int? targetMarkazId)
        {
            switch (userCodeRole)
            {
                case 1:
                case 2:
                    return true;

                case 3:
                    if (!targetMarkazId.HasValue || string.IsNullOrEmpty(userOstanId))
                        return false;
                    return true;

                case 4:
                    if (!userMarkazId.HasValue || !targetMarkazId.HasValue)
                        return false;
                    return userMarkazId == targetMarkazId;

                default:
                    return false;
            }
        }
    }
}