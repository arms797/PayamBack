using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SignatureController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        public SignatureController(AppDbContext context, 
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // 1️⃣ دریافت امضای کاربر فعلی
        // ============================================================
        [HttpGet("my-signature")]
        public async Task<IActionResult> GetMySignature()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "کاربر یافت نشد" });

                var signature = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == userId.Value);

                if (signature == null)
                    return Ok(new
                    {
                        success = true,
                        message = "امضایی برای این کاربر یافت نشد",
                        data = (object?)null
                    });

                return Ok(new
                {
                    success = true,
                    message = "امضا دریافت شد",
                    data = new
                    {
                        signature.Id,
                        signature.Signature,
                        signature.Position,
                        signature.CanEditSignature,
                        signature.CanEditPosition,
                        signature.CreatedAt,
                        signature.UpdatedAt,
                        HasSignature = !string.IsNullOrEmpty(signature.Signature)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت امضا",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت امضای یک کاربر (فقط ادمین)
        // ============================================================
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSignature(int userId)
        {
            try
            {
                var signature = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (signature == null)
                    return NotFound(new { success = false, message = "امضایی برای این کاربر یافت نشد" });

                return Ok(new
                {
                    success = true,
                    message = "امضا دریافت شد",
                    data = new
                    {
                        signature.Id,
                        signature.Signature,
                        signature.Position,
                        signature.CanEditSignature,
                        signature.CanEditPosition,
                        signature.CreatedAt,
                        signature.UpdatedAt,
                        HasSignature = !string.IsNullOrEmpty(signature.Signature)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت امضا",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ذخیره/بروزرسانی امضا
        // ============================================================
        [HttpPost("save")]
        public async Task<IActionResult> SaveSignature([FromBody] SaveSignatureDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "کاربر یافت نشد" });

                var user = await _userManager.FindByIdAsync(userId.Value.ToString());
                if (user == null)
                    return NotFound(new { success = false, message = "کاربر یافت نشد" });

                var existing = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == userId.Value);

                if (existing != null)
                {
                    // ============================================================
                    // اگر امضا وجود دارد، بررسی کن که آیا کاربر اجازه ویرایش دارد
                    // ============================================================
                    if (existing.CanEditSignature==false && !string.IsNullOrEmpty(existing.Signature))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "شما اجازه ویرایش امضا را ندارید. برای فعال‌سازی با ادمین تماس بگیرید."
                        });
                    }

                    // بروزرسانی
                    existing.Signature = dto.Signature;
                    existing.Position = dto.Position ?? existing.Position;
                    existing.UpdatedAt = DateTime.UtcNow;

                    // ============================================================
                    // بعد از ویرایش، قفل می‌شود (فقط ادمین می‌تواند دوباره باز کند)
                    // ============================================================
                    existing.CanEditSignature = false;
                    existing.CanEditPosition = false;

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "امضا با موفقیت به‌روزرسانی شد"
                    });
                }

                // ============================================================
                // ایجاد امضا جدید (اولین بار)
                // ============================================================
                var signature = new UserSignature
                {
                    UserId = userId.Value,
                    Signature = dto.Signature,
                    Position = dto.Position ?? "BC",
                    CanEditSignature = false,
                    CanEditPosition = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.UserSignatures.AddAsync(signature);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "امضا با موفقیت ذخیره شد"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ذخیره امضا در دیتابیس",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ذخیره امضا",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ تغییر موقعیت امضا (بدون تغییر خود امضا)
        // ============================================================
        [HttpPatch("change-position")]
        public async Task<IActionResult> ChangePosition([FromBody] ChangePositionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "کاربر یافت نشد" });

                var signature = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == userId.Value);

                if (signature == null)
                    return NotFound(new { success = false, message = "امضایی برای این کاربر یافت نشد" });

                // ============================================================
                // بررسی اجازه ویرایش موقعیت
                // ============================================================
                if (signature.CanEditPosition==false)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "شما اجازه تغییر موقعیت امضا را ندارید. برای فعال‌سازی با ادمین تماس بگیرید."
                    });
                }

                // تغییر موقعیت
                signature.Position = dto.Position;
                signature.UpdatedAt = DateTime.UtcNow;

                // ============================================================
                // بعد از تغییر، قفل می‌شود
                // ============================================================
                signature.CanEditPosition = false;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "موقعیت امضا با موفقیت تغییر کرد"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر موقعیت امضا در دیتابیس",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر موقعیت امضا",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ باز کردن قفل ویرایش (فقط ادمین)
        // ============================================================
        [HttpPatch("unlock")]
        public async Task<IActionResult> UnlockSignature([FromBody] UnlockSignatureDto dto)
        {
            try
            {
                var signature = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == dto.UserId);

                if (signature == null)
                    return NotFound(new { success = false, message = "امضایی برای این کاربر یافت نشد" });

                // ============================================================
                // باز کردن قفل بر اساس نوع
                // ============================================================
                if (dto.UnlockType == "position" || dto.UnlockType == "both")
                {
                    signature.CanEditPosition = true;
                }

                if (dto.UnlockType == "signature" || dto.UnlockType == "both")
                {
                    signature.CanEditSignature = true;
                }

                signature.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var typeText = dto.UnlockType switch
                {
                    "position" => "موقعیت",
                    "signature" => "امضا",
                    "both" => "موقعیت و امضا",
                    _ => "امضا"
                };

                return Ok(new
                {
                    success = true,
                    message = $"دسترسی ویرایش {typeText} برای کاربر با شناسه {dto.UserId} فعال شد"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در باز کردن قفل در دیتابیس",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در باز کردن قفل امضا",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ حذف امضا (فقط ادمین)
        // ============================================================
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            try
            {
                var signature = await _context.UserSignatures
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (signature == null)
                    return NotFound(new { success = false, message = "امضایی برای این کاربر یافت نشد" });

                _context.UserSignatures.Remove(signature);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "امضا با موفقیت حذف شد"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف امضا از دیتابیس",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف امضا",
                    error = ex.Message
                });
            }
        }


        // در SignatureController

        [HttpPost("get-multiple")]
        public async Task<IActionResult> GetMultipleSignatures([FromBody] GetMultipleSignaturesDto dto)
        {
            try
            {
                if (dto.UserIds == null || !dto.UserIds.Any())
                    return BadRequest(new { success = false, message = "لیست کاربران خالی است" });

                // حذف UserIdهای تکراری
                var uniqueUserIds = dto.UserIds.Distinct().ToList();

                // دریافت همه امضاها
                var signatures = await _context.UserSignatures
                    .Where(s => uniqueUserIds.Contains(s.UserId))
                    .ToDictionaryAsync(
                        s => s.UserId,
                        s => new { s.Signature, s.Position }
                    );

                // ساخت پاسخ با پوشش دادن همه UserIdها
                var result = new Dictionary<int, object?>();
                foreach (var userId in uniqueUserIds)
                {
                    if (signatures.TryGetValue(userId, out var sig))
                    {
                        result[userId] = new
                        {
                            sig.Signature,
                            sig.Position
                        };
                    }
                    else
                    {
                        result[userId] = null; // کاربر امضا ندارد یا وجود ندارد
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "امضاها دریافت شد",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت امضاها",
                    error = ex.Message
                });
            }
        }

        public class GetMultipleSignaturesDto
        {
            [Required]
            public List<int> UserIds { get; set; } = new();
        }
        // ============================================================
        // متد کمکی: دریافت شناسه کاربر فعلی
        // ============================================================
        private int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return null;
                return userId;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // 1️⃣ دریافت لیست کاربرانی که امضا دارند (بر اساس سطح دسترسی)
        // ============================================================
        [HttpGet("users")]
        public async Task<IActionResult> ManageSignatureForReset(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                // ============================================================
                // 1️⃣ دریافت اطلاعات کاربر فعلی و نقش فعال
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 2️⃣ دریافت مراکز قابل دسترس بر اساس CodeRole
                // ============================================================
                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);
                if (!accessibleMarkazIds.Any())
                    return Ok(new
                    {
                        success = true,
                        message = "شما دسترسی به هیچ مرکزی ندارید",
                        data = new List<object>(),
                        pagination = new { page, pageSize, totalCount = 0, totalPages = 0 }
                    });

                // ============================================================
                // 3️⃣ 🔥 کوئری بهینه (بدون Include اضافی)
                // ============================================================
                var query = _userManager.Users
                    .Where(u => _context.UserSignatures
                        .Any(s => s.UserId == u.Id && !string.IsNullOrEmpty(s.Signature))
                    )
                    .AsQueryable();

                // ============================================================
                // 4️⃣ فیلتر بر اساس مراکز قابل دسترس
                // ============================================================
                if (codeRole != 1 && codeRole != 2)
                {
                    query = query.Where(u =>
                        (u.Ostad != null && u.Ostad.MarkazId.HasValue && accessibleMarkazIds.Contains(u.Ostad.MarkazId.Value)) ||
                        (u.Karmand != null && u.Karmand.MarkazId.HasValue && accessibleMarkazIds.Contains(u.Karmand.MarkazId.Value)) ||
                        (u.Daneshjoo != null && u.Daneshjoo.MarkazId.HasValue && accessibleMarkazIds.Contains(u.Daneshjoo.MarkazId.Value)) ||
                        (u.MoshakhasatAdmin != null)
                    );
                }

                // ============================================================
                // 5️⃣ جستجو
                // ============================================================
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.Contains(search)) ||
                        (u.Ostad != null && (u.Ostad.Naam != null && u.Ostad.Naam.Contains(search))) ||
                        (u.Ostad != null && (u.Ostad.NaamKhanevadegi != null && u.Ostad.NaamKhanevadegi.Contains(search))) ||
                        (u.Karmand != null && (u.Karmand.Naam != null && u.Karmand.Naam.Contains(search))) ||
                        (u.Karmand != null && (u.Karmand.NaameKhanevadeghi != null && u.Karmand.NaameKhanevadeghi.Contains(search))) ||
                        (u.Daneshjoo != null && (u.Daneshjoo.Naam != null && u.Daneshjoo.Naam.Contains(search))) ||
                        (u.Daneshjoo != null && (u.Daneshjoo.NaamKhanevadegi != null && u.Daneshjoo.NaamKhanevadegi.Contains(search))) ||
                        (u.MoshakhasatAdmin != null && (u.MoshakhasatAdmin.Naam != null && u.MoshakhasatAdmin.Naam.Contains(search))) ||
                        (u.MoshakhasatAdmin != null && (u.MoshakhasatAdmin.NaameKhanevadeghi != null && u.MoshakhasatAdmin.NaameKhanevadeghi.Contains(search)))
                    );
                }

                // ============================================================
                // 6️⃣ صفحه‌بندی
                // ============================================================
                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        FirstName = u.Ostad != null ? u.Ostad.Naam :
                                    u.Karmand != null ? u.Karmand.Naam :
                                    u.Daneshjoo != null ? u.Daneshjoo.Naam :
                                    u.MoshakhasatAdmin != null ? u.MoshakhasatAdmin.Naam : "",
                        LastName = u.Ostad != null ? u.Ostad.NaamKhanevadegi :
                                   u.Karmand != null ? u.Karmand.NaameKhanevadeghi :
                                   u.Daneshjoo != null ? u.Daneshjoo.NaamKhanevadegi :
                                   u.MoshakhasatAdmin != null ? u.MoshakhasatAdmin.NaameKhanevadeghi : "",
                        MarkazName = u.Ostad != null && u.Ostad.Markaz != null ? u.Ostad.Markaz.NaamMarkaz :
                                     u.Karmand != null && u.Karmand.Markaz != null ? u.Karmand.Markaz.NaamMarkaz :
                                     u.Daneshjoo != null && u.Daneshjoo.Markaz != null ? u.Daneshjoo.Markaz.NaamMarkaz :
                                     "بدون مرکز"
                    })
                    .ToListAsync();

                // ============================================================
                // 7️⃣ ساخت نتیجه نهایی
                // ============================================================
                var result = users.Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.MarkazName,
                    HasSignature = true // چون فقط کاربران با امضا در لیست هستند
                }).ToList();

                return Ok(new
                {
                    success = true,
                    message = "لیست کاربران دارای امضا دریافت شد",
                    data = result,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست کاربران",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // متد کمکی: دریافت اطلاعات کاربر فعلی
        // ============================================================
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

        // ============================================================
        // متد کمکی: دریافت مراکز قابل دسترس
        // ============================================================
        private async Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1)
            {
                // ادمین سامانه: همه مراکز
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 2)
            {
                // ادمین سازمان: مراکز سطح 2 و 3
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true && (m.Level == 2 || m.Level == 3))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null)
                return new List<int>();

            if (codeRole == 3)
            {
                // ادمین استان: استان خودش و مراکز آن استان
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true &&
                        (m.Level == 3 && m.CodeOstan == currentMarkaz.CodeOstan) ||
                        (m.Level == 4 && m.CodeOstan == currentMarkaz.CodeOstan))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 4)
            {
                // ادمین مرکز: فقط مرکز خودش
                return new List<int> { currentMarkaz.Id };
            }

            return new List<int>();
        }
    }

    // ============================================================
    // DTOها
    // ============================================================

    public class SaveSignatureDto
    {
        [Required]
        public string Signature { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Position { get; set; } = "BC";
    }

    public class ChangePositionDto
    {
        [Required]
        [MaxLength(50)]
        public string Position { get; set; } = "BC";
    }

    public class UnlockSignatureDto
    {
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// "position" فقط موقعیت | "signature" فقط امضا | "both" هر دو
        /// </summary>
        [MaxLength(10)]
        public string UnlockType { get; set; } = "both";
    }
}