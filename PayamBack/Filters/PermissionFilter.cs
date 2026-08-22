// PayamBack/Filters/PermissionFilter.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Filters
{
    public class PermissionFilter : IAsyncActionFilter
    {
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly IAccessService _accessService;

        public PermissionFilter(
            IMemoryCache cache,
            ICurrentUserService currentUserService,
            IPermissionCacheService permissionCacheService,
            IAccessService accessService)
        {
            _cache = cache;
            _currentUserService = currentUserService;
            _permissionCacheService = permissionCacheService;
            _accessService = accessService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ============================================================
            // 1️⃣ بررسی AllowAnonymous
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
            // 2️⃣ بررسی احراز هویت (با استفاده از ICurrentUserService)
            // ============================================================
            var (user, _, _, _) = await _currentUserService.GetCurrentUserInfoAsync();
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // ============================================================
            // 3️⃣ بررسی NoPermission (نادیده گرفتن مجوز)
            // ============================================================
            var noPermission = endpoint?.Metadata?.GetMetadata<NoPermissionAttribute>() != null;
            if (noPermission)
            {
                await next();
                return;
            }

            // ============================================================
            // 4️⃣ دریافت نقش فعال از JWT
            // ============================================================
            var roleClaims = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var activeRoleName = roleClaims.FirstOrDefault();

            if (string.IsNullOrEmpty(activeRoleName))
            {
                context.Result = new ForbidResult();
                return;
            }

            // ============================================================
            // 5️⃣ دریافت RoleId از Cache (با استفاده از IMemoryCache)
            // ============================================================
            var roleCacheKey = $"RoleId_{activeRoleName}";
            var activeRoleId = _cache.Get<int?>(roleCacheKey);

            if (!activeRoleId.HasValue)
            {
                activeRoleId = await _accessService.GetRoleIdByNameAsync(activeRoleName);
                if (!activeRoleId.HasValue)
                {
                    context.Result = new ForbidResult();
                    return;
                }
                _cache.Set(roleCacheKey, activeRoleId.Value, TimeSpan.FromDays(1));
            }

            // ============================================================
            // 6️⃣ دریافت نام کنترلر و اکشن
            // ============================================================
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
            var actionName = context.ActionDescriptor.RouteValues["action"] ?? "";

            // ============================================================
            // 7️⃣ نرمال‌سازی actionName به View, Create, Update, Delete
            // ============================================================
            var normalizedAction = NormalizeAction(actionName);
            var permissionName = $"{controllerName}.{normalizedAction}";
            var wildcardPermissionName = $"{controllerName}.*";

            // ============================================================
            // 8️⃣ بررسی مجوز از کش با استفاده از PermissionCacheService
            // ============================================================
            var permissionCacheKey = $"Permission_{activeRoleId.Value}_{permissionName}";
            var hasPermission = _cache.Get<bool?>(permissionCacheKey);

            if (!hasPermission.HasValue)
            {
                // دریافت همه مجوزهای نقش از سرویس کش
                var rolePermissions = await _permissionCacheService.GetRolePermissionsAsync(activeRoleId.Value);
                hasPermission = rolePermissions.Contains(permissionName) ||
                                rolePermissions.Contains(wildcardPermissionName);

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

            // ============================================================
            // ✅ مجوز تأیید شد، ادامه بده
            // ============================================================
            await next();
        }

        // ============================================================
        // متد نرمال‌سازی اکشن‌ها
        // ============================================================
        private string NormalizeAction(string action)
        {
            // 1️⃣ خواندن → View
            if (action.StartsWith("Get") ||
                action == "List" || action == "All" || action == "Active" ||
                action == "Inactive" || action == "Search" || action == "Filter" ||
                action == "Index" || action == "Details")
                return "View";

            // 2️⃣ ایجاد → Create
            if (action == "Create" || action == "Add" || action == "Insert" || action == "Register")
                return "Create";

            // 3️⃣ ویرایش → Update
            if (action == "Update" || action == "Edit" || action == "Modify" ||
                action == "Change" || action == "Toggle" || action == "Active" ||
                action == "Deactive" || action == "Activate" || action == "Deactivate" ||
                action == "ResetPassword" || action == "ToggleStatus")
                return "Update";

            // 4️⃣ حذف → Delete
            if (action == "Delete" || action == "Remove" || action == "Archive")
                return "Delete";

            // 5️⃣ BulkUpload → مجوز خاص
            if (action == "BulkUpload")
                return "BulkUpload";

            return action;
        }
    }
}