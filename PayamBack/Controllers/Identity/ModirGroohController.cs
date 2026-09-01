using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ModirGroohController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;

        public ModirGroohController(
            AppDbContext context,
            ICurrentUserService currentUserService,
            IAccessService accessService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _accessService = accessService;
        }

        // ============================================================
        // 1️⃣ دریافت لیست مدیرگروه ها (با فیلتر)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int? userId = null,
            [FromQuery] int? grooheId = null,
            [FromQuery] bool? vazeeat = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // فقط ادمین سامانه می‌تواند لیست کامل را ببیند
                if (codeRole != 1)
                    return Forbid();

                var query = _context.ModirGroohs
                    .Include(mg => mg.AppUserRole)
                        .ThenInclude(ur => ur.User)
                    .Include(mg => mg.AppUserRole)
                        .ThenInclude(ur => ur.Role)
                    .Include(mg => mg.AppUserRole)
                        .ThenInclude(ur => ur.Markaz)
                    .Include(mg => mg.GrooheAmoozeshi)
                    .AsQueryable();

                if (userId.HasValue)
                    query = query.Where(mg => mg.AppUserRole.UserId == userId.Value);

                if (grooheId.HasValue)
                    query = query.Where(mg => mg.GrooheAmoozeshiId == grooheId.Value);

                if (vazeeat.HasValue)
                    query = query.Where(mg => mg.Vazeeat == vazeeat.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(mg => mg.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(mg => new
                    {
                        mg.Id,
                        UserId = mg.AppUserRole.UserId,
                        UserName = mg.AppUserRole.User.UserName,
                        RoleName = mg.AppUserRole.Role.Name,
                        MarkazName = mg.AppUserRole.Markaz != null ? mg.AppUserRole.Markaz.NaamMarkaz : "",
                        GrooheName = mg.GrooheAmoozeshi.OnvanGrooheAmoozeshi,
                        mg.Vazeeat,
                        mg.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مدیران گروه دریافت شد",
                    data = items,
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
                    message = "خطا در دریافت لیست مدیران گروه",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ ایجاد مدیر گروه جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ModirGroohCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                // بررسی وجود AppUserRole
                var appUserRole = await _context.Set<AppUserRole>()
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.Id == dto.AppUserRoleId);

                if (appUserRole == null)
                    return BadRequest(new { success = false, message = "نقش کاربر یافت نشد" });

                // فقط نقش‌های با CodeRole=3 یا 4 می‌توانند مدیر گروه باشند
                if (appUserRole.Role?.CodeRole != 3 && appUserRole.Role?.CodeRole != 4)
                    return BadRequest(new { success = false, message = "این نقش نمی‌تواند مدیر گروه باشد" });

                // بررسی وجود گروه آموزشی
                var groohe = await _context.GrooheAmoozeshis
                    .FirstOrDefaultAsync(g => g.Id == dto.GrooheAmoozeshiId);

                if (groohe == null)
                    return BadRequest(new { success = false, message = "گروه آموزشی یافت نشد" });

                // بررسی تکراری نبودن
                var exists = await _context.ModirGroohs
                    .AnyAsync(mg => mg.AppUserRoleId == dto.AppUserRoleId
                                    && mg.GrooheAmoozeshiId == dto.GrooheAmoozeshiId);

                if (exists)
                    return BadRequest(new { success = false, message = "این مدیر گروه قبلاً ثبت شده است" });

                var modirGrooh = new ModirGrooh
                {
                    AppUserRoleId = dto.AppUserRoleId,
                    GrooheAmoozeshiId = dto.GrooheAmoozeshiId,
                    Vazeeat = dto.Vazeeat ?? true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.ModirGroohs.AddAsync(modirGrooh);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدیر گروه با موفقیت ایجاد شد",
                    data = new { id = modirGrooh.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد مدیر گروه",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ حذف مدیر گروه
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                var modirGrooh = await _context.ModirGroohs
                    .FirstOrDefaultAsync(mg => mg.Id == id);

                if (modirGrooh == null)
                    return NotFound(new { success = false, message = "مدیر گروه یافت نشد" });

                _context.ModirGroohs.Remove(modirGrooh);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدیر گروه با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف مدیر گروه",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ تغییر وضعیت مدیر گروه
        // ============================================================
        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                var modirGrooh = await _context.ModirGroohs
                    .FirstOrDefaultAsync(mg => mg.Id == id);

                if (modirGrooh == null)
                    return NotFound(new { success = false, message = "مدیر گروه یافت نشد" });

                modirGrooh.Vazeeat = !modirGrooh.Vazeeat;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"وضعیت مدیر گروه با موفقیت به {(modirGrooh.Vazeeat ? "فعال" : "غیرفعال")} تغییر کرد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت مدیر گروه",
                    error = ex.Message
                });
            }
        }
    }

    // ============================================================
    // DTOها
    // ============================================================

    public class ModirGroohCreateDto
    {
        [Required]
        public int AppUserRoleId { get; set; }

        [Required]
        public int GrooheAmoozeshiId { get; set; }

        public bool? Vazeeat { get; set; }
    }
}