// RoleAssignmentController.cs - نسخه اصلاح‌شده
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity.RoleAssignment;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleAssignmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;
        private readonly IMarkazCacheService _markazCache;

        public RoleAssignmentController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ICurrentUserService currentUserService,
            IAccessService accessService,
            IMarkazCacheService markazCache)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _accessService = accessService;
            _markazCache = markazCache;
        }

        // ============================================================
        // 🔥 متد کمکی جدید با استفاده از سرویس‌ها
        // ============================================================

        /// <summary>بررسی مجاز بودن نقش برای کاربر فعلی در مرکز مشخص</summary>
        private async Task<bool> CanAssignRoleAsync(int roleId, int codeRole, int markazId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return false;

            var allMarkaz = await _markazCache.GetAllAsync();
            var targetMarkaz = allMarkaz.FirstOrDefault(m => m.Id == markazId);
            if (targetMarkaz == null) return false;

            // ============================================================
            // 1️⃣ ادمین سامانه (CodeRole=1) → همه نقش‌ها
            // ============================================================
            if (codeRole == 1) return true;

            // ============================================================
            // 2️⃣ ادمین سازمان (CodeRole=2)
            // ============================================================
            if (codeRole == 2)
            {
                if (role.IsAdmin != true)
                {
                    if (role.CodeRole == 2 && targetMarkaz.Level == 2) return true;
                    if (role.CodeRole == 3 && targetMarkaz.Level == 3) return true;
                    return false;
                }

                if (role.IsAdmin == true && role.CodeRole == 3 && targetMarkaz.Level == 3) return true;
                return false;
            }

            // ============================================================
            // 3️⃣ ادمین استان (CodeRole=3)
            // ============================================================
            if (codeRole == 3)
            {
                if (role.IsAdmin != true)
                {
                    if (role.CodeRole == 3 && targetMarkaz.Level == 3) return true;
                    if (role.CodeRole == 4 && targetMarkaz.Level == 4) return true;
                    return false;
                }

                if (role.IsAdmin == true && role.CodeRole == 4 && targetMarkaz.Level == 4) return true;
                return false;
            }

            // ============================================================
            // 4️⃣ ادمین مرکز (CodeRole=4)
            // ============================================================
            if (codeRole == 4)
            {
                if (role.IsAdmin == true) return false;
                if (role.CodeRole == 4 && targetMarkaz.Level == 4) return true;
                return false;
            }

            return false;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه انتصاب‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? userId = null,
            [FromQuery] int? roleId = null,
            [FromQuery] int? markazId = null,
            [FromQuery] bool? isDefault = null)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);
                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                var query = _context.Set<AppUserRole>()
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .Include(ur => ur.Markaz)
                    .Include(ur => ur.ParentUserRole)
                        .ThenInclude(p => p.User)
                    .Where(ur => ur.MarkazId.HasValue && accessibleMarkazIds.Contains(ur.MarkazId.Value))
                    .AsQueryable();

                if (userId.HasValue)
                    query = query.Where(ur => ur.UserId == userId.Value);

                if (roleId.HasValue)
                    query = query.Where(ur => ur.RoleId == roleId.Value);

                if (markazId.HasValue)
                    query = query.Where(ur => ur.MarkazId == markazId.Value);

                if (isDefault.HasValue)
                    query = query.Where(ur => ur.RolePishFarz == isDefault.Value);

                var totalCount = await query.CountAsync();

                var assignments = await query
                    .OrderBy(ur => ur.User.UserName)
                    .ThenBy(ur => ur.Role.CodeRole)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ur => new RoleAssignmentListDto
                    {
                        Id = ur.Id,
                        UserId = ur.UserId,
                        UserName = ur.User != null ? ur.User.UserName ?? "" : "",
                        UserFullName = ur.User != null ?
                            (ur.User.Karmand != null ? $"{ur.User.Karmand.Naam} {ur.User.Karmand.NaameKhanevadeghi}" :
                             ur.User.Ostad != null ? $"{ur.User.Ostad.Naam} {ur.User.Ostad.NaamKhanevadegi}" :
                             ur.User.Daneshjoo != null ? $"{ur.User.Daneshjoo.Naam} {ur.User.Daneshjoo.NaamKhanevadegi}" :
                             ur.User.MoshakhasatAdmin != null ? $"{ur.User.MoshakhasatAdmin.Naam} {ur.User.MoshakhasatAdmin.NaameKhanevadeghi}" :
                             "") : "",
                        RoleId = ur.RoleId,
                        RoleName = ur.Role != null ? ur.Role.Name ?? "" : "",
                        CodeRole = ur.Role != null ? ur.Role.CodeRole ?? 4 : 4,
                        IsAdmin = ur.Role != null ? ur.Role.IsAdmin ?? false : false,
                        MarkazId = ur.MarkazId ?? 0,
                        MarkazName = ur.Markaz != null ? ur.Markaz.NaamMarkaz ?? "" : "",
                        MarkazLevel = ur.Markaz != null ? ur.Markaz.Level ?? 4 : 4,
                        IsDefault = ur.RolePishFarz ?? false,
                        ParentUserRoleId = ur.ParentUserRoleId,
                        ParentUserName = ur.ParentUserRole != null && ur.ParentUserRole.User != null ?
                            ur.ParentUserRole.User.UserName ?? "" : "",
                        ParentUserFullName = ur.ParentUserRole != null && ur.ParentUserRole.User != null ?
                            (ur.ParentUserRole.User.Karmand != null ? $"{ur.ParentUserRole.User.Karmand.Naam} {ur.ParentUserRole.User.Karmand.NaameKhanevadeghi}" :
                             ur.ParentUserRole.User.Ostad != null ? $"{ur.ParentUserRole.User.Ostad.Naam} {ur.ParentUserRole.User.Ostad.NaamKhanevadegi}" :
                             "") : ""
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست انتصاب‌ها دریافت شد",
                    data = assignments,
                    pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت لیست", error = ex.Message });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک انتصاب با شناسه
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var assignment = await _context.Set<AppUserRole>()
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .Include(ur => ur.Markaz)
                    .Include(ur => ur.ParentUserRole)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(ur => ur.Id == id);

                if (assignment == null)
                    return NotFound(new { success = false, message = "انتصاب یافت نشد" });

                var dto = new RoleAssignmentDetailDto
                {
                    Id = assignment.Id,
                    UserId = assignment.UserId,
                    UserName = assignment.User?.UserName ?? "",
                    RoleId = assignment.RoleId,
                    RoleName = assignment.Role?.Name ?? "",
                    CodeRole = assignment.Role?.CodeRole ?? 4,
                    IsAdmin = assignment.Role?.IsAdmin ?? false,
                    IsUniquePerMarkaz = assignment.Role?.IsUniquePerMarkaz ?? false,
                    MarkazId = assignment.MarkazId ?? 0,
                    MarkazName = assignment.Markaz?.NaamMarkaz ?? "",
                    MarkazLevel = assignment.Markaz?.Level ?? 4,
                    IsDefault = assignment.RolePishFarz ?? false,
                    ParentUserRoleId = assignment.ParentUserRoleId,
                    ParentUserName = assignment.ParentUserRole?.User?.UserName ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات انتصاب دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت اطلاعات", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ دریافت نقش‌های یک کاربر
        // ============================================================
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                var assignments = await _context.Set<AppUserRole>()
                    .Include(ur => ur.Role)
                    .Include(ur => ur.Markaz)
                    .Include(ur => ur.ParentUserRole)
                        .ThenInclude(p => p.User)
                    .Where(ur => ur.UserId == userId && ur.MarkazId.HasValue && accessibleMarkazIds.Contains(ur.MarkazId.Value))
                    .Select(ur => new RoleAssignmentListDto
                    {
                        Id = ur.Id,
                        UserId = ur.UserId,
                        RoleId = ur.RoleId,
                        RoleName = ur.Role != null ? ur.Role.Name ?? "" : "",
                        CodeRole = ur.Role != null ? ur.Role.CodeRole ?? 4 : 4,
                        IsAdmin = ur.Role != null ? ur.Role.IsAdmin ?? false : false,
                        MarkazId = ur.MarkazId ?? 0,
                        MarkazName = ur.Markaz != null ? ur.Markaz.NaamMarkaz ?? "" : "",
                        MarkazLevel = ur.Markaz != null ? ur.Markaz.Level ?? 4 : 4,
                        IsDefault = ur.RolePishFarz ?? false,
                        ParentUserRoleId = ur.ParentUserRoleId,
                        ParentUserName = ur.ParentUserRole != null && ur.ParentUserRole.User != null ?
                            ur.ParentUserRole.User.UserName ?? "" : ""
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "نقش‌های کاربر دریافت شد",
                    data = assignments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت نقش‌های کاربر", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ دریافت نقش‌های یک مرکز
        // ============================================================
        [HttpGet("by-markaz/{markazId}")]
        public async Task<IActionResult> GetByMarkaz(int markazId)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await _accessService.CanAccessTargetMarkazAsync(markazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var assignments = await _context.Set<AppUserRole>()
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .Where(ur => ur.MarkazId == markazId)
                    .Select(ur => new RoleAssignmentListDto
                    {
                        Id = ur.Id,
                        UserId = ur.UserId,
                        UserName = ur.User != null ? ur.User.UserName ?? "" : "",
                        RoleId = ur.RoleId,
                        RoleName = ur.Role != null ? ur.Role.Name ?? "" : "",
                        CodeRole = ur.Role != null ? ur.Role.CodeRole ?? 4 : 4,
                        IsAdmin = ur.Role != null ? ur.Role.IsAdmin ?? false : false,
                        MarkazId = ur.MarkazId ?? 0,
                        IsDefault = ur.RolePishFarz ?? false,
                        ParentUserRoleId = ur.ParentUserRoleId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "نقش‌های مرکز دریافت شد",
                    data = assignments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت نقش‌های مرکز", error = ex.Message });
            }
        }

        // ============================================================
        // 5️⃣ دریافت نقش‌های قابل تخصیص برای یک مرکز
        // ============================================================
        [HttpGet("assignable-roles/{markazId}")]
        public async Task<IActionResult> GetAssignableRoles(int markazId)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await _accessService.CanAccessTargetMarkazAsync(markazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var allRoles = await _roleManager.Roles
                    .Where(r => r.Vazeeyat == true)
                    .ToListAsync();

                var assignableRoles = new List<object>();

                foreach (var role in allRoles)
                {
                    var canAssign = await CanAssignRoleAsync(role.Id, codeRole.Value, markazId);
                    if (canAssign)
                    {
                        assignableRoles.Add(new
                        {
                            role.Id,
                            role.Name,
                            role.CodeRole,
                            role.IsAdmin,
                            role.IsUniquePerMarkaz
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "نقش‌های قابل تخصیص دریافت شد",
                    data = assignableRoles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت نقش‌های قابل تخصیص", error = ex.Message });
            }
        }

        // ============================================================
        // 6️⃣ دریافت نقش‌های اختصاص‌یافته به مرکز (برای انتخاب والد)
        // ============================================================
        [HttpGet("assigned-roles/{markazId}")]
        public async Task<IActionResult> GetAssignedRoles(int markazId)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await _accessService.CanAccessTargetMarkazAsync(markazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var assignedRoles = await _context.Set<AppUserRole>()
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .Where(ur => ur.MarkazId == markazId)
                    .Select(ur => new
                    {
                        ur.Id,
                        UserId = ur.UserId,
                        UserName = ur.User != null ? ur.User.UserName ?? "" : "",
                        UserFullName = ur.User != null ?
                            (ur.User.Karmand != null ? $"{ur.User.Karmand.Naam} {ur.User.Karmand.NaameKhanevadeghi}" :
                             ur.User.Ostad != null ? $"{ur.User.Ostad.Naam} {ur.User.Ostad.NaamKhanevadegi}" :
                             ur.User.Daneshjoo != null ? $"{ur.User.Daneshjoo.Naam} {ur.User.Daneshjoo.NaamKhanevadegi}" :
                             ur.User.MoshakhasatAdmin != null ? $"{ur.User.MoshakhasatAdmin.Naam} {ur.User.MoshakhasatAdmin.NaameKhanevadeghi}" :
                             "") : "",
                        RoleId = ur.RoleId,
                        RoleName = ur.Role != null ? ur.Role.Name ?? "" : "",
                        CodeRole = ur.Role != null ? ur.Role.CodeRole ?? 4 : 4,
                        IsDefault = ur.RolePishFarz ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "نقش‌های اختصاص‌یافته به مرکز دریافت شد",
                    data = assignedRoles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت نقش‌های اختصاص‌یافته", error = ex.Message });
            }
        }

        // ============================================================
        // 7️⃣ ایجاد انتصاب جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RoleAssignmentCreateDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 🔥 بررسی دسترسی به مرکز
                // ============================================================
                if (!await _accessService.CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // ============================================================
                // 🔥 بررسی مجاز بودن نقش برای این کاربر
                // ============================================================
                if (!await CanAssignRoleAsync(dto.RoleId, codeRole.Value, dto.MarkazId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "شما مجوز اختصاص این نقش را در این مرکز ندارید"
                    });
                }

                // ============================================================
                // 1️⃣ اعتبارسنجی
                // ============================================================
                var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
                if (user == null)
                    return BadRequest(new { success = false, message = "کاربر یافت نشد" });

                var role = await _roleManager.FindByIdAsync(dto.RoleId.ToString());
                if (role == null)
                    return BadRequest(new { success = false, message = "نقش یافت نشد" });

                var markaz = await _context.Markazes.FindAsync(dto.MarkazId);
                if (markaz == null)
                    return BadRequest(new { success = false, message = "مرکز یافت نشد" });

                // بررسی تکراری نبودن
                var exists = await _context.Set<AppUserRole>()
                    .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId && ur.MarkazId == dto.MarkazId);

                if (exists)
                    return BadRequest(new { success = false, message = "این نقش قبلاً به این کاربر در این مرکز اختصاص داده شده است" });

                // ============================================================
                // 🔥 بررسی Unique بودن نقش در مرکز
                // ============================================================
                if (role.IsUniquePerMarkaz == true)
                {
                    var existingUnique = await _context.Set<AppUserRole>()
                        .AnyAsync(ur => ur.RoleId == dto.RoleId && ur.MarkazId == dto.MarkazId);

                    if (existingUnique)
                        return BadRequest(new { success = false, message = $"نقش '{role.Name}' در این مرکز قبلاً به کاربر دیگری اختصاص داده شده است" });
                }

                // ============================================================
                // 🔥 بررسی ParentUserRole
                // ============================================================
                AppUserRole? parent = null;
                if (dto.ParentUserRoleId.HasValue)
                {
                    parent = await _context.Set<AppUserRole>()
                        .Include(ur => ur.Role)
                        .FirstOrDefaultAsync(ur => ur.Id == dto.ParentUserRoleId.Value);

                    if (parent == null)
                        return BadRequest(new { success = false, message = "رکورد والد یافت نشد" });

                    if (parent.MarkazId != dto.MarkazId)
                        return BadRequest(new { success = false, message = "والد باید در همان مرکز باشد" });

                    var childRole = await _roleManager.FindByIdAsync(dto.RoleId.ToString());
                    var parentRole = parent.Role;

                    if (parentRole == null || childRole == null)
                        return BadRequest(new { success = false, message = "نقش والد یا فرزند یافت نشد" });

                    // ✅ کد نقش والد باید کمتر از کد نقش فرزند باشد
                    if (parentRole.CodeRole > childRole.CodeRole)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"نقش '{parentRole.Name}' (کد {parentRole.CodeRole}) نمی‌تواند والد نقش '{childRole.Name}' (کد {childRole.CodeRole}) باشد. کد نقش والد باید کمتر از کد نقش فرزند باشد."
                        });
                    }
                }

                // ============================================================
                // 2️⃣ ایجاد انتصاب
                // ============================================================
                var assignment = new AppUserRole
                {
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    MarkazId = dto.MarkazId,
                    RolePishFarz = dto.IsDefault ?? false,
                    ParentUserRoleId = dto.ParentUserRoleId
                };

                await _context.Set<AppUserRole>().AddAsync(assignment);
                await _context.SaveChangesAsync();

                // ============================================================
                // 3️⃣ اگر IsDefault == true، نقش‌های پیش‌فرض دیگر را غیرفعال کن
                // ============================================================
                if (dto.IsDefault == true)
                {
                    var otherDefaults = await _context.Set<AppUserRole>()
                        .Where(ur => ur.UserId == dto.UserId && ur.Id != assignment.Id && ur.RolePishFarz == true)
                        .ToListAsync();

                    foreach (var other in otherDefaults)
                    {
                        other.RolePishFarz = false;
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "نقش با موفقیت به کاربر اختصاص داده شد",
                    data = new { id = assignment.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد انتصاب", error = ex.Message });
            }
        }

        // ============================================================
        // 8️⃣ ویرایش انتصاب (فقط IsDefault و ParentUserRoleId)
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleAssignmentUpdateDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ پیدا کردن انتصاب
                // ============================================================
                var assignment = await _context.Set<AppUserRole>()
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.Id == id);

                if (assignment == null)
                    return NotFound(new { success = false, message = "انتصاب یافت نشد" });

                // بررسی دسترسی به مرکز
                if (!await _accessService.CanAccessTargetMarkazAsync(assignment.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // ============================================================
                // 2️⃣ ویرایش فیلدها
                // ============================================================

                // 🔥 تغییر وضعیت پیش‌فرض
                if (dto.IsDefault.HasValue && dto.IsDefault.Value != assignment.RolePishFarz)
                {
                    assignment.RolePishFarz = dto.IsDefault.Value;

                    if (dto.IsDefault.Value == true)
                    {
                        var otherDefaults = await _context.Set<AppUserRole>()
                            .Where(ur => ur.UserId == assignment.UserId && ur.Id != assignment.Id && ur.RolePishFarz == true)
                            .ToListAsync();

                        foreach (var other in otherDefaults)
                        {
                            other.RolePishFarz = false;
                        }
                    }
                }

                // ============================================================
                // 🔥 تغییر ParentUserRole
                // ============================================================
                if (dto.ParentUserRoleId.HasValue && dto.ParentUserRoleId.Value != assignment.ParentUserRoleId)
                {
                    if (dto.ParentUserRoleId.Value > 0)
                    {
                        var parent = await _context.Set<AppUserRole>()
                            .Include(ur => ur.Role)
                            .FirstOrDefaultAsync(ur => ur.Id == dto.ParentUserRoleId.Value);

                        if (parent == null)
                            return BadRequest(new { success = false, message = "رکورد والد یافت نشد" });

                        if (parent.MarkazId != assignment.MarkazId)
                            return BadRequest(new { success = false, message = "والد باید در همان مرکز باشد" });

                        if (parent.Id == assignment.Id)
                            return BadRequest(new { success = false, message = "یک انتصاب نمی‌تواند والد خودش باشد" });

                        var parentRole = parent.Role;
                        var childRole = await _roleManager.FindByIdAsync(assignment.RoleId.ToString());

                        if (parentRole == null || childRole == null)
                            return BadRequest(new { success = false, message = "نقش والد یا فرزند یافت نشد" });

                        // ✅ کد نقش والد باید کمتر از کد نقش فرزند باشد
                        if (parentRole.CodeRole > childRole.CodeRole)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = $"نقش '{parentRole.Name}' (کد {parentRole.CodeRole}) نمی‌تواند والد نقش '{childRole.Name}' (کد {childRole.CodeRole}) باشد. کد نقش والد باید کمتر از کد نقش فرزند باشد."
                            });
                        }
                    }

                    assignment.ParentUserRoleId = dto.ParentUserRoleId.Value > 0 ? dto.ParentUserRoleId.Value : null;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "انتصاب با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش انتصاب", error = ex.Message });
            }
        }

        // ============================================================
        // 9️⃣ حذف انتصاب
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // ============================================================
                // 🔥 بررسی دسترسی (فقط ادمین سامانه)
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                // ============================================================
                // 1️⃣ پیدا کردن انتصاب
                // ============================================================
                var assignment = await _context.Set<AppUserRole>()
                    .FirstOrDefaultAsync(ur => ur.Id == id);

                if (assignment == null)
                    return NotFound(new { success = false, message = "انتصاب یافت نشد" });

                // بررسی اینکه آیا این انتصاب والد برای انتصاب دیگری است
                var hasChildren = await _context.Set<AppUserRole>()
                    .AnyAsync(ur => ur.ParentUserRoleId == id);

                if (hasChildren)
                    return BadRequest(new { success = false, message = "این انتصاب دارای زیردست است. ابتدا زیردستان را حذف کنید" });

                _context.Set<AppUserRole>().Remove(assignment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "انتصاب با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف انتصاب", error = ex.Message });
            }
        }

        // ============================================================
        // 🔟 تنظیم نقش پیش‌فرض
        // ============================================================
        [HttpPatch("set-default/{id}")]
        public async Task<IActionResult> SetDefault(int id)
        {
            try
            {
                // ============================================================
                // 🔥 بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ پیدا کردن انتصاب
                // ============================================================
                var assignment = await _context.Set<AppUserRole>()
                    .FirstOrDefaultAsync(ur => ur.Id == id);

                if (assignment == null)
                    return NotFound(new { success = false, message = "انتصاب یافت نشد" });

                // بررسی دسترسی به مرکز
                if (!await _accessService.CanAccessTargetMarkazAsync(assignment.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // ============================================================
                // 2️⃣ تنظیم به‌عنوان پیش‌فرض
                // ============================================================
                var otherDefaults = await _context.Set<AppUserRole>()
                    .Where(ur => ur.UserId == assignment.UserId && ur.RolePishFarz == true && ur.Id != id)
                    .ToListAsync();

                foreach (var other in otherDefaults)
                {
                    other.RolePishFarz = false;
                }

                assignment.RolePishFarz = true;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "نقش به‌عنوان پیش‌فرض تنظیم شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در تنظیم نقش پیش‌فرض", error = ex.Message });
            }
        }
    }
}