using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity.Permission;
using PayamBack.Models.Identity;
using System.Reflection;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ادمین سامانه")]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه مجوزها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var permissions = await _context.Permissions
                    .OrderBy(p => p.Resource)
                    .ThenBy(p => p.Action)
                    .Select(p => new PermissionListDto
                    {
                        Id = p.Id,
                        Resource = p.Resource ?? "",
                        Action = p.Action ?? "",
                        Name = p.Name ?? "",
                        Description = p.Description ?? "",
                        IsActive = p.IsActive ?? false,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مجوزها با موفقیت دریافت شد",
                    data = permissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست مجوزها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک مجوز
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var permission = await _context.Permissions
                    .Where(p => p.Id == id)
                    .Select(p => new PermissionDetailDto
                    {
                        Id = p.Id,
                        Resource = p.Resource ?? "",
                        Action = p.Action ?? "",
                        Name = p.Name ?? "",
                        Description = p.Description ?? "",
                        IsActive = p.IsActive ?? false,
                        CreatedAt = p.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (permission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مجوز مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات مجوز با موفقیت دریافت شد",
                    data = permission
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات مجوز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد مجوز جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PermissionCreateDto dto)
        {
            try
            {
                // بررسی تکراری نبودن Name
                var exists = await _context.Permissions
                    .AnyAsync(p => p.Name == dto.Name);

                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "مجوز با این نام قبلاً ثبت شده است"
                    });
                }

                var permission = new Permission
                {
                    Resource = dto.Resource,
                    Action = dto.Action,
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Permissions.AddAsync(permission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مجوز با موفقیت ایجاد شد",
                    data = new { id = permission.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد مجوز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش مجوز
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionUpdateDto dto)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (permission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مجوز مورد نظر یافت نشد"
                    });
                }

                // بررسی تکراری نبودن Name (اگر تغییر کرده باشد)
                if (!string.IsNullOrEmpty(dto.Name) && dto.Name != permission.Name)
                {
                    var exists = await _context.Permissions
                        .AnyAsync(p => p.Name == dto.Name && p.Id != id);

                    if (exists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "مجوز با این نام قبلاً ثبت شده است"
                        });
                    }
                }

                // به‌روزرسانی فیلدها
                permission.Resource = dto.Resource ?? permission.Resource;
                permission.Action = dto.Action ?? permission.Action;
                permission.Name = dto.Name ?? permission.Name;
                permission.Description = dto.Description ?? permission.Description;
                permission.IsActive = dto.IsActive ?? permission.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مجوز با موفقیت بروزرسانی شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بروزرسانی مجوز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف مجوز
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (permission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مجوز مورد نظر یافت نشد"
                    });
                }

                // بررسی اینکه مجوز در RolePermission استفاده نشده باشد
                var isUsed = await _context.RolePermissions
                    .AnyAsync(rp => rp.PermissionId == id);

                if (isUsed)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این مجوز به یک یا چند نقش اختصاص داده شده است و قابل حذف نیست"
                    });
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مجوز با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف مجوز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ دریافت مجوزهای یک نقش
        // ============================================================
        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            try
            {
                var permissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId && rp.Vazeeat == true)
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => new PermissionListDto
                        {
                            Id = p.Id,
                            Resource = p.Resource ?? "",
                            Action = p.Action ?? "",
                            Name = p.Name ?? "",
                            Description = p.Description ?? "",
                            IsActive = p.IsActive ?? false,
                            CreatedAt = p.CreatedAt
                        })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مجوزهای نقش با موفقیت دریافت شد",
                    data = permissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت مجوزهای نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 🔥 7️⃣ دریافت لیست همه کنترلرها و اکشن‌های نرمال‌سازی‌شده
        // ============================================================
        [HttpGet("actions-list")]
        public IActionResult GetActionsList()
        {
            try
            {
                // ============================================================
                // 1️⃣ دریافت همه کنترلرها از اسمبلی جاری
                // ============================================================
                var controllers = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.IsClass &&
                                !t.IsAbstract &&
                                typeof(ControllerBase).IsAssignableFrom(t) &&
                                t.Namespace != null &&
                                t.Namespace.StartsWith("PayamBack.Controllers"))
                    .ToList();

                var result = new List<ControllerActionDto>();

                foreach (var controller in controllers)
                {
                    // ============================================================
                    // 2️⃣ دریافت نام کنترلر (بدون پسوند "Controller")
                    // ============================================================
                    var controllerName = controller.Name.Replace("Controller", "");

                    // ============================================================
                    // 3️⃣ دریافت همه متدهای عمومی (اکشن‌ها)
                    // ============================================================
                    var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(m => m.IsPublic &&
                                    !m.IsSpecialName &&  // حذف getter/setter
                                    m.DeclaringType == controller &&  // فقط متدهای خود کنترلر
                                    m.ReturnType != typeof(void) &&  // متدهایی که مقدار برمی‌گردانند
                                    m.GetCustomAttributes<NonActionAttribute>().Count() == 0 &&  // حذف [NonAction]
                                    m.GetCustomAttributes<AllowAnonymousAttribute>().Count() == 0) // حذف [AllowAnonymous]
                        .ToList();

                    foreach (var method in methods)
                    {
                        // ============================================================
                        // 4️⃣ نرمال‌سازی نام اکشن
                        // ============================================================
                        var actionName = method.Name;
                        var normalizedAction = NormalizeAction(actionName);

                        // ============================================================
                        // 5️⃣ اضافه کردن به لیست (با شرط یکتا)
                        // ============================================================
                        var existing = result.FirstOrDefault(r => r.Resource == controllerName && r.Action == normalizedAction);
                        if (existing == null)
                        {
                            result.Add(new ControllerActionDto
                            {
                                Resource = controllerName,
                                Action = normalizedAction,
                                PermissionName = $"{controllerName}.{normalizedAction}"
                            });
                        }
                    }
                }

                // ============================================================
                // 6️⃣ مرتب‌سازی بر اساس Resource و Action
                // ============================================================
                result = result
                    .OrderBy(r => r.Resource)
                    .ThenBy(r => r.Action)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "لیست کنترلرها و اکشن‌های نرمال‌سازی‌شده دریافت شد",
                    data = result,
                    totalCount = result.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست اکشن‌ها",
                    error = ex.Message
                });
            }
        }
        // ============================================================
        // 🔥 متد نرمال‌سازی اکشن‌ها (همانند PermissionFilter)
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
            if (action == "Delete" || action == "Remove" || action == "Deactivate" || action == "Archive")
                return "Delete";

            // 5️⃣ BulkUpload → مجوز خاص
            if (action == "BulkUpload")
                return "BulkUpload";

            return action;
        }
    }

    // ============================================================
    // DTO برای خروجی
    // ============================================================
    public class ControllerActionDto
    {
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
    }
}