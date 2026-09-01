using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.OstadMadrak;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // ← همه اکشن‌ها نیاز به احراز هویت دارند
    public class OstadMadrakController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;

        public OstadMadrakController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService=currentUserService;
        }


        // ============================================================
        // 1️⃣ دریافت مدارک یک استاد
        // ============================================================
        [HttpGet("by-ostad/{ostadId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByOstadId(int ostadId)
        {
            try
            {
                var madraks = await _context.OstadMadraks
                    .Include(m => m.GrooheAmoozeshi)
                    .Include(m => m.CreatedByUser)
                        .ThenInclude(u => u.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Include(m => m.CreatedByUser)
                        .ThenInclude(u => u.Karmand)
                        .ThenInclude(k => k.Markaz)
                    .Include(m => m.CreatedByUser)
                        .ThenInclude(u => u.Daneshjoo)
                        .ThenInclude(d => d.Markaz)
                    .Include(m => m.CreatedByUser)
                        .ThenInclude(u => u.MoshakhasatAdmin)
                    .Include(m => m.ApprovedByUser)
                        .ThenInclude(u => u.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Include(m => m.ApprovedByUser)
                        .ThenInclude(u => u.Karmand)
                        .ThenInclude(k => k.Markaz)
                    .Include(m => m.ApprovedByUser)
                        .ThenInclude(u => u.Daneshjoo)
                        .ThenInclude(d => d.Markaz)
                    .Include(m => m.ApprovedByUser)
                        .ThenInclude(u => u.MoshakhasatAdmin)
                    .Where(m => m.OstadId == ostadId)
                    .Select(m => new OstadMadrakListDto
                    {
                        Id = m.Id,
                        OstadId = m.OstadId ?? 0,
                        Reshteh = m.Reshteh ?? "",
                        Grayesh = m.Grayesh ?? "",
                        Maghta = m.Maghta ?? 0,
                        PishFarz = m.PishFarz ?? false,
                        MahalAkhz = m.MahalAkhz ?? "",
                        TasvirMadrak = m.TasvirMadrak ?? "",
                        GrooheAmoozeshiId = m.GrooheAmoozeshiId ?? 0,
                        GrooheAmoozeshiName = m.GrooheAmoozeshi != null ? m.GrooheAmoozeshi.OnvanGrooheAmoozeshi ?? "" : "",

                        // ============================================================
                        // 🔥 اطلاعات ایجاد کننده (بر اساس نوع کاربر)
                        // ============================================================
                        CreatedByUserId = m.CreatedByUserId,
                        CreatedByUserInfo = m.CreatedByUser != null ?
                            (m.CreatedByUser.Ostad != null ?
                                $"{m.CreatedByUser.Ostad.Naam} {m.CreatedByUser.Ostad.NaamKhanevadegi}" :
                             m.CreatedByUser.Karmand != null ?
                                $"{m.CreatedByUser.Karmand.Naam} {m.CreatedByUser.Karmand.NaameKhanevadeghi}" :
                             m.CreatedByUser.Daneshjoo != null ?
                                $"{m.CreatedByUser.Daneshjoo.Naam} {m.CreatedByUser.Daneshjoo.NaamKhanevadegi}" :
                             m.CreatedByUser.MoshakhasatAdmin != null ?
                                $"{m.CreatedByUser.MoshakhasatAdmin.Naam} {m.CreatedByUser.MoshakhasatAdmin.NaameKhanevadeghi}" :
                                m.CreatedByUser.UserName ?? "") :
                            "",
                        CreatedByRoleInfo = m.CreatedByRoleInfo ?? "",
                        CreatedAt = m.CreatedAt,

                        // ============================================================
                        // 🔥 اطلاعات تایید کننده (بر اساس نوع کاربر)
                        // ============================================================
                        IsApproved = m.IsApproved ?? false,
                        ApprovedByUserId = m.ApprovedByUserId,
                        ApprovedByUserInfo = m.ApprovedByUser != null ?
                            (m.ApprovedByUser.Ostad != null ?
                                $"{m.ApprovedByUser.Ostad.Naam} {m.ApprovedByUser.Ostad.NaamKhanevadegi}" :
                             m.ApprovedByUser.Karmand != null ?
                                $"{m.ApprovedByUser.Karmand.Naam} {m.ApprovedByUser.Karmand.NaameKhanevadeghi}" :
                             m.ApprovedByUser.Daneshjoo != null ?
                                $"{m.ApprovedByUser.Daneshjoo.Naam} {m.ApprovedByUser.Daneshjoo.NaamKhanevadegi}" :
                             m.ApprovedByUser.MoshakhasatAdmin != null ?
                                $"{m.ApprovedByUser.MoshakhasatAdmin.Naam} {m.ApprovedByUser.MoshakhasatAdmin.NaameKhanevadeghi}" :
                                m.ApprovedByUser.UserName ?? "") :
                            "",
                        ApprovedByRoleInfo = m.ApprovedByRoleInfo ?? "",
                        ApprovedAt = m.ApprovedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدارک استاد دریافت شد",
                    data = madraks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت مدارک",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ ایجاد مدرک جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OstadMadrakCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var madrak = new OstadMadrak
                {
                    OstadId = dto.OstadId,
                    Reshteh = dto.Reshteh,
                    Grayesh = dto.Grayesh,
                    Maghta = dto.Maghta,
                    PishFarz = dto.PishFarz ?? false,
                    MahalAkhz = dto.MahalAkhz,
                    TasvirMadrak = dto.TasvirMadrak,
                    GrooheAmoozeshiId = dto.GrooheAmoozeshiId,
                    CreatedByUserId = currentUser.Id,
                    CreatedByRoleInfo = currentRole.Name,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false,
                    ApprovedByUserId = null,
                    ApprovedByRoleInfo = null,
                    ApprovedAt = null
                };

                if (madrak.PishFarz == true)
                {
                    await _context.OstadMadraks
                        .Where(m => m.OstadId == dto.OstadId && m.PishFarz == true)
                        .ForEachAsync(m => m.PishFarz = false);
                }

                await _context.OstadMadraks.AddAsync(madrak);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدرک با موفقیت ایجاد شد. در انتظار تایید.",
                    data = new { id = madrak.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ تایید مدرک
        // ============================================================
        [HttpPatch("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var madrak = await _context.OstadMadraks.FindAsync(id);
                if (madrak == null)
                    return NotFound(new { success = false, message = "مدرک یافت نشد" });

                if (madrak.IsApproved == true)
                    return BadRequest(new { success = false, message = "این مدرک قبلاً تایید شده است" });

                madrak.IsApproved = true;
                madrak.ApprovedByUserId = currentUser.Id;
                madrak.ApprovedByRoleInfo = currentRole.Name;
                madrak.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدرک با موفقیت تایید شد",
                    data = new
                    {
                        id = madrak.Id,
                        isApproved = madrak.IsApproved,
                        approvedByUserId = madrak.ApprovedByUserId,
                        approvedByRoleInfo = madrak.ApprovedByRoleInfo,
                        approvedAt = madrak.ApprovedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در تایید مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ لغو تایید مدرک
        // ============================================================
        [HttpPatch("unapprove/{id}")]
        public async Task<IActionResult> Unapprove(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var madrak = await _context.OstadMadraks.FindAsync(id);
                if (madrak == null)
                    return NotFound(new { success = false, message = "مدرک یافت نشد" });

                if (madrak.IsApproved != true)
                    return BadRequest(new { success = false, message = "این مدرک تایید نشده است" });

                madrak.IsApproved = false;
                madrak.ApprovedByUserId = null;
                madrak.ApprovedByRoleInfo = null;
                madrak.ApprovedAt = null;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تایید مدرک با موفقیت لغو شد",
                    data = new
                    {
                        id = madrak.Id,
                        isApproved = madrak.IsApproved
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در لغو تایید مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 5️⃣ ویرایش مدرک
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OstadMadrakUpdateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var madrak = await _context.OstadMadraks.FindAsync(id);
                if (madrak == null)
                    return NotFound(new { success = false, message = "مدرک یافت نشد" });

                // ============================================================
                // 🔥 بررسی دسترسی برای ویرایش - بر اساس مجوز (PermissionFilter انجام می‌دهد)
                // ============================================================
                // اگر مدرک تایید شده باشد، فقط کاربرانی که مجوز "OstadMadrak.Approve" دارند
                // می‌توانند ویرایش کنند (PermissionFilter این را بررسی می‌کند)

                if (madrak.IsApproved == true)
                {
                    // فقط کاربرانی با مجوز مناسب می‌توانند ویرایش کنند
                    // (PermissionFilter مجوز را بررسی می‌کند)
                }

                madrak.Reshteh = dto.Reshteh ?? madrak.Reshteh;
                madrak.Grayesh = dto.Grayesh ?? madrak.Grayesh;
                madrak.Maghta = dto.Maghta ?? madrak.Maghta;
                madrak.PishFarz = dto.PishFarz ?? madrak.PishFarz;
                madrak.MahalAkhz = dto.MahalAkhz ?? madrak.MahalAkhz;
                madrak.TasvirMadrak = dto.TasvirMadrak ?? madrak.TasvirMadrak;
                madrak.GrooheAmoozeshiId = dto.GrooheAmoozeshiId ?? madrak.GrooheAmoozeshiId;

                if (madrak.PishFarz == true)
                {
                    await _context.OstadMadraks
                        .Where(m => m.OstadId == madrak.OstadId && m.Id != madrak.Id && m.PishFarz == true)
                        .ForEachAsync(m => m.PishFarz = false);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدرک با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 6️⃣ حذف مدرک
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var madrak = await _context.OstadMadraks.FindAsync(id);
                if (madrak == null)
                    return NotFound(new { success = false, message = "مدرک یافت نشد" });

                // ============================================================
                // 🔥 بررسی دسترسی برای حذف - بر اساس مجوز (PermissionFilter انجام می‌دهد)
                // ============================================================

                _context.OstadMadraks.Remove(madrak);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدرک با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 7️⃣ دریافت مدارک تایید نشده (برای نمایش به کاربران دارای مجوز)
        // ============================================================
        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApproval()
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 🔥 بدون بررسی codeRole - PermissionFilter مجوز را بررسی می‌کند
                // ============================================================

                var madraks = await _context.OstadMadraks
                    .Include(m => m.Ostad)
                    .Include(m => m.CreatedByUser)
                    .Where(m => m.IsApproved == false)
                    .Select(m => new
                    {
                        m.Id,
                        m.OstadId,
                        OstadName = m.Ostad != null ? $"{m.Ostad.Naam} {m.Ostad.NaamKhanevadegi}" : "",
                        m.Reshteh,
                        m.Maghta,
                        m.CreatedByUserId,
                        CreatedByUserName = m.CreatedByUser != null ? m.CreatedByUser.UserName : "",
                        m.CreatedByRoleInfo,
                        m.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "مدارک در انتظار تایید دریافت شد",
                    data = madraks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت مدارک در انتظار تایید", error = ex.Message });
            }
        }
    }
}