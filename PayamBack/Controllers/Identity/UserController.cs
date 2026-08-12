using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.Security.Claims;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // 🔥 متدهای کمکی (همانند RoleAssignmentController)
        // ============================================================

        /// <summary>دریافت اطلاعات کاربر فعلی و نقش فعال</summary>
        private async Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (null, null, null, null);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, null, null, null);

            var roleName = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleName))
                return (user, null, null, null);

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return (user, null, null, null);

            var activeRole = await _context.Set<AppUserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

            Markaz? markaz = null;
            if (activeRole?.MarkazId != null)
            {
                markaz = await _context.Markazes.FindAsync(activeRole.MarkazId.Value);
            }

            return (user, role, markaz, role.CodeRole);
        }

        /// <summary>بررسی دسترسی به مرکز هدف</summary>
        private async Task<bool> CanAccessTargetMarkazAsync(int targetMarkazId, int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1) return true;

            var targetMarkaz = await _context.Markazes.FindAsync(targetMarkazId);
            if (targetMarkaz == null) return false;

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null) return false;

            if (codeRole == 2)
                return targetMarkaz.Level == 2 || targetMarkaz.Level == 3;

            if (codeRole == 3)
                return targetMarkaz.Level == 3 || (targetMarkaz.Level == 4 && targetMarkaz.CodeOstan == currentMarkaz.CodeOstan);

            if (codeRole == 4)
                return targetMarkaz.Id == currentMarkaz.Id;

            return false;
        }

        // ============================================================
        // 1️⃣ دریافت کاربر بر اساس نوع و شناسه
        // ============================================================
        [HttpGet("by-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByType(
            [FromQuery] string type,
            [FromQuery] int id)
        {
            try
            {
                if (string.IsNullOrEmpty(type))
                    return BadRequest(new { success = false, message = "نوع کاربر مشخص نشده است" });

                AppUser? user = null;

                // ============================================================
                // 🔥 انتخاب فیلد بر اساس نوع کاربر
                // ============================================================
                switch (type.ToLower())
                {
                    case "karmand":
                        user = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.KarmandId == id);
                        break;
                    case "ostad":
                        user = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.OstadId == id);
                        break;
                    case "daneshjoo":
                        user = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.DaneshjooId == id);
                        break;
                    case "admin":
                        user = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.AdminId == id);
                        break;
                    default:
                        return BadRequest(new { success = false, message = "نوع کاربر نامعتبر است" });
                }

                if (user == null)
                    return NotFound(new { success = false, message = $"کاربری برای این {type} یافت نشد" });

                // ============================================================
                // 🔥 دریافت اطلاعات نام و نام خانوادگی بر اساس نوع
                // ============================================================
                string? firstName = null;
                string? lastName = null;

                switch (type.ToLower())
                {
                    case "karmand":
                        var karmand = await _context.Karmands
                            .FirstOrDefaultAsync(k => k.Id == id);
                        if (karmand != null)
                        {
                            firstName = karmand.Naam;
                            lastName = karmand.NaameKhanevadeghi;
                        }
                        break;
                    case "ostad":
                        var ostad = await _context.Ostads
                            .FirstOrDefaultAsync(o => o.Id == id);
                        if (ostad != null)
                        {
                            firstName = ostad.Naam;
                            lastName = ostad.NaamKhanevadegi;
                        }
                        break;
                    case "daneshjoo":
                        var daneshjoo = await _context.Daneshjoos
                            .FirstOrDefaultAsync(d => d.Id == id);
                        if (daneshjoo != null)
                        {
                            firstName = daneshjoo.Naam;
                            lastName = daneshjoo.NaamKhanevadegi;
                        }
                        break;
                    case "admin":
                        var admin = await _context.MoshakhasatAdmins
                            .FirstOrDefaultAsync(a => a.Id == id);
                        if (admin != null)
                        {
                            firstName = admin.Naam;
                            lastName = admin.NaameKhanevadeghi;
                        }
                        break;
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات کاربر دریافت شد",
                    data = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.Vazeeyat,
                        user.VazeeyatMovaghat,
                        FirstName = firstName,
                        LastName = lastName,
                        UserType = type.ToLower()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت اطلاعات کاربر", error = ex.Message });
            }
        }

        // ============================================================
        // 2️⃣ تغییر وضعیت کاربر (فعال/غیرفعال و وضعیت موقت)
        // ============================================================
        [HttpPatch("toggle-status/{userId}")]
        public async Task<IActionResult> ToggleStatus(int userId, [FromBody] ToggleUserStatusDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ پیدا کردن کاربر هدف
                // ============================================================
                var targetUser = await _userManager.Users
                    .Include(u => u.Karmand)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (targetUser == null)
                    return NotFound(new { success = false, message = "کاربر یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی به مرکز کاربر
                // ============================================================
                if (targetUser.Karmand?.MarkazId != null)
                {
                    if (!await CanAccessTargetMarkazAsync(targetUser.Karmand.MarkazId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }
                else
                {
                    // اگر کاربر به هیچ مرکزی متصل نیست، فقط ادمین سامانه اجازه دارد
                    if (codeRole != 1)
                        return Forbid();
                }

                // ============================================================
                // 3️⃣ تغییر وضعیت
                // ============================================================
                if (dto.Vazeeyat.HasValue)
                    targetUser.Vazeeyat = dto.Vazeeyat.Value;

                if (dto.VazeeyatMovaghat.HasValue)
                    targetUser.VazeeyatMovaghat = dto.VazeeyatMovaghat.Value;

                var result = await _userManager.UpdateAsync(targetUser);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در تغییر وضعیت کاربر",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "وضعیت کاربر با موفقیت تغییر کرد",
                    data = new
                    {
                        userId = targetUser.Id,
                        userName = targetUser.UserName,
                        vazeeyat = targetUser.Vazeeyat,
                        vazeeyatMovaghat = targetUser.VazeeyatMovaghat
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در تغییر وضعیت کاربر", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ریست رمز عبور کاربر
        // ============================================================
        [HttpPost("reset-password/{userId}")]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ پیدا کردن کاربر هدف
                // ============================================================
                var targetUser = await _userManager.Users
                    .Include(u => u.Karmand)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (targetUser == null)
                    return NotFound(new { success = false, message = "کاربر یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی به مرکز کاربر
                // ============================================================
                if (targetUser.Karmand?.MarkazId != null)
                {
                    if (!await CanAccessTargetMarkazAsync(targetUser.Karmand.MarkazId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }
                else
                {
                    if (codeRole != 1)
                        return Forbid();
                }

                // ============================================================
                // 3️⃣ ریست رمز عبور
                // ============================================================
                if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "رمز عبور جدید باید حداقل ۶ کاراکتر باشد"
                    });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
                var result = await _userManager.ResetPasswordAsync(targetUser, token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ریست رمز عبور",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "رمز عبور با موفقیت ریست شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ریست رمز عبور", error = ex.Message });
            }
        }
        

        // ============================================================
        // 4️⃣ دریافت اطلاعات یک کاربر با شناسه
        // ============================================================
        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userManager.Users
                    .Include(u => u.Karmand)
                    .Include(u => u.Ostad)
                    .Include(u => u.Daneshjoo)
                    .Include(u => u.MoshakhasatAdmin)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new { success = false, message = "کاربر یافت نشد" });

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات کاربر دریافت شد",
                    data = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.Vazeeyat,
                        user.VazeeyatMovaghat,
                        user.KarmandId,
                        user.OstadId,
                        user.DaneshjooId,
                        user.AdminId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت اطلاعات کاربر", error = ex.Message });
            }
        }
        
    }

    // ============================================================
    // 🔥 DTOها
    // ============================================================

    public class ToggleUserStatusDto
    {
        public bool? Vazeeyat { get; set; }
        public bool? VazeeyatMovaghat { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
        
}