using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Schedule.ElmiTerm;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Schedule
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ElmiTermController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;

        public ElmiTermController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            ICurrentUserService currentUserService,
            IAccessService accessService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _currentUserService = currentUserService;
            _accessService = accessService;
        }

        // ============================================================
        // 🔥 متدهای کمکی (فقط موارد ضروری)
        // ============================================================

        private string GetRoleMarkazDisplay(AppRole? role, Markaz? markaz)
        {
            var roleName = role?.Name ?? "نقش نامشخص";
            var markazName = "مرکز نامشخص";

            if (markaz != null)
            {
                if (markaz.Level == 2)
                    markazName = "سازمان مرکزی";
                else if (markaz.Level == 3)
                    markazName = $"استان {markaz.NaamOstan ?? ""}";
                else
                    markazName = markaz.NaamMarkaz ?? "مرکز";
            }

            return $"{roleName} - {markazName}";
        }

        private static string GetOstadName(AppUser? user)
        {
            if (user?.Ostad == null) return "";
            return $"{user.Ostad.Naam} {user.Ostad.NaamKhanevadegi}".Trim();
        }

        private static string GetOstadCode(AppUser? user)
        {
            return user?.Ostad?.CodeOstadi ?? "";
        }

        private static string GetOstadMarkaz(AppUser? user)
        {
            return user?.Ostad?.Markaz?.NaamMarkaz ?? "";
        }

        private static string GetApproveStatusDisplay(int? status)
        {
            return status switch
            {
                0 => "در انتظار بررسی",
                1 => "تایید شده",
                2 => "رد شده",
                _ => "نامشخص"
            };
        }

        private string GetUserFullName(AppUser? user)
        {
            if (user == null) return "";

            if (user.Ostad != null)
                return $"{user.Ostad.Naam} {user.Ostad.NaamKhanevadegi}".Trim();

            if (user.Karmand != null)
                return $"{user.Karmand.Naam} {user.Karmand.NaameKhanevadeghi}".Trim();

            if (user.MoshakhasatAdmin != null)
                return $"{user.MoshakhasatAdmin.Naam} {user.MoshakhasatAdmin.NaameKhanevadeghi}".Trim();

            if (user.Daneshjoo != null)
                return $"{user.Daneshjoo.Naam} {user.Daneshjoo.NaamKhanevadegi}".Trim();

            return user.UserName ?? "";
        }

        private async Task<string> SaveFileAsync(IFormFile file, int id)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "uploads", "elmi-term");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/elmi-term/{fileName}";
        }

        private void DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", filePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        // ============================================================
        // 1️⃣ دریافت لیست درخواست‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? approveStatus = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                var query = from e in _context.Set<ElmiTerm>()
                            join u in _context.Users on e.UserId equals u.Id into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            join o in _context.Ostads on u.OstadId equals o.Id into ostadJoin
                            from o in ostadJoin.DefaultIfEmpty()
                            join au in _context.Users on e.ApprovedByUserId equals au.Id into approvedJoin
                            from au in approvedJoin.DefaultIfEmpty()
                            select new { ElmiTerm = e, User = u, Ostad = o, ApprovedUser = au };

                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    query = query.Where(x => x.ElmiTerm.UserId == currentUser.Id);
                }
                else if (codeRole == 3 && currentMarkaz != null)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == currentMarkaz.CodeOstan)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad != null &&
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (codeRole == 4 && currentMarkaz != null)
                {
                    query = query.Where(x =>
                        x.Ostad != null &&
                        x.Ostad.MarkazId == currentMarkaz.Id);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.Ostad != null && x.Ostad.Naam != null && x.Ostad.Naam.Contains(search)) ||
                        (x.Ostad != null && x.Ostad.NaamKhanevadegi != null && x.Ostad.NaamKhanevadegi.Contains(search)) ||
                        (x.Ostad != null && x.Ostad.CodeOstadi != null && x.Ostad.CodeOstadi.Contains(search)));
                }

                if (approveStatus.HasValue)
                {
                    query = query.Where(x => x.ElmiTerm.ApproveStatus == approveStatus.Value);
                }

                if (ostanId.HasValue && !markazId.HasValue && !isOstad)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad != null &&
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue && !isOstad)
                {
                    query = query.Where(x =>
                        x.Ostad != null &&
                        x.Ostad.MarkazId == markazId.Value);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.ElmiTerm.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ElmiTermListDto
                    {
                        Id = x.ElmiTerm.Id,
                        UserId = x.ElmiTerm.UserId,
                        OstadName = x.Ostad != null ? $"{x.Ostad.Naam} {x.Ostad.NaamKhanevadegi}" : "",
                        OstadCode = x.Ostad != null ? x.Ostad.CodeOstadi ?? "" : "",
                        OstadMarkaz = x.Ostad != null && x.Ostad.MarkazId != null ?
                            _context.Markazes.Where(m => m.Id == x.Ostad.MarkazId).Select(m => m.NaamMarkaz ?? "").FirstOrDefault() ?? "" : "",
                        AkharinVazeeat = x.ElmiTerm.AkharinVazeeat ?? "",
                        IsEjeari = x.ElmiTerm.IsEjeari ?? false,
                        OnvanEjraei = x.ElmiTerm.OnvanEjraei ?? "",
                        FullTime = x.ElmiTerm.FullTime ?? false,
                        TedadSaatMovazafi = x.ElmiTerm.TedadSaatMovazafi,
                        TedadVahedMovazafi = x.ElmiTerm.TedadVahedMovazafi,
                        Vazeeat = x.ElmiTerm.Vazeeat,
                        ApproveStatus = x.ElmiTerm.ApproveStatus ?? 0,
                        ApproveStatusDisplay = GetApproveStatusDisplay(x.ElmiTerm.ApproveStatus),
                        ApprovedBy = x.ApprovedUser != null ? GetOstadName(x.ApprovedUser) : "",
                        HasFile = !string.IsNullOrEmpty(x.ElmiTerm.FilePath),
                        FilePath = x.ElmiTerm.FilePath ?? "",
                        CreatedAt = DateTime.Now
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست درخواست‌ها دریافت شد",
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
                    message = "خطا در دریافت لیست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک درخواست
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _context.Set<ElmiTerm>()
                    .Include(e => e.User)
                        .ThenInclude(u => u.Ostad)
                            .ThenInclude(o => o.Markaz)
                    .Include(e => e.ApprovedByUser)
                        .ThenInclude(u => u.Ostad)
                    .Include(e => e.UserSabtKonandeh)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (item == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var isOstad = currentRole?.Name == "استاد";
                if (isOstad && item.UserId != currentUser.Id)
                    return Forbid();

                if (!isOstad && item.UserId.HasValue)
                {
                    if (!await _accessService.CanAccessTargetUserAsync(item.UserId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                var dto = new ElmiTermDetailDto
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    OstadName = GetOstadName(item.User),
                    OstadCode = GetOstadCode(item.User),
                    OstadMarkaz = GetOstadMarkaz(item.User),
                    AkharinVazeeat = item.AkharinVazeeat ?? "",
                    IsEjeari = item.IsEjeari ?? false,
                    OnvanEjraei = item.OnvanEjraei ?? "",
                    FullTime = item.FullTime ?? false,
                    TedadSaatMovazafi = item.TedadSaatMovazafi,
                    TedadVahedMovazafi = item.TedadVahedMovazafi,
                    Vazeeat = item.Vazeeat,
                    ApproveStatus = item.ApproveStatus ?? 0,
                    ApproveStatusDisplay = GetApproveStatusDisplay(item.ApproveStatus),
                    ApprovedByUserName = item.ApprovedByUser != null ? GetUserFullName(item.ApprovedByUser) : "",
                    ApprovedByRoleMarkaz = item.ApprovedByRoleMarkaz ?? "",
                    ApprovedAt = item.ApprovedAt,
                    ApproveTozihat = item.ApproveTozihat,
                    FilePath = item.FilePath,
                    FileName = Path.GetFileName(item.FilePath),
                    CreatedBy = item.UserSabtKonandeh != null ? GetUserFullName(item.UserSabtKonandeh) : "",
                    CreatedAt = DateTime.Now
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات درخواست دریافت شد",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد درخواست جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ElmiTermCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var isOstad = currentRole?.Name == "استاد";

                if (isOstad && dto.UserId != currentUser.Id)
                    return BadRequest(new { success = false, message = "شما فقط می‌توانید درخواست خود را ثبت کنید" });

                if (!isOstad)
                {
                    if (!await _accessService.CanAccessTargetUserAsync(dto.UserId, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                // ============================================================
                // 🔥 ثبت پیش‌نویس جدید (غیرفعال)
                // ============================================================
                var newEntity = new ElmiTerm
                {
                    UserId = dto.UserId,
                    UserIdSabtKonandeh = currentUser.Id,
                    RoleMarkazSabtKonandeh = currentRole?.Id,
                    AkharinVazeeat = dto.AkharinVazeeat,
                    IsEjeari = dto.IsEjeari,
                    OnvanEjraei = dto.OnvanEjraei,
                    FullTime = dto.FullTime,
                    TedadSaatMovazafi = dto.TedadSaatMovazafi,
                    TedadVahedMovazafi = dto.TedadVahedMovazafi,
                    ApproveStatus = 0,          // در انتظار بررسی
                    Vazeeat = false             // ❌ هنوز فعال نشده (پیش‌نویس)
                };

                await _context.Set<ElmiTerm>().AddAsync(newEntity);
                await _context.SaveChangesAsync();

                if (dto.File != null)
                {
                    newEntity.FilePath = await SaveFileAsync(dto.File, newEntity.Id);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "پیش‌نویس با موفقیت ثبت شد",
                    data = new { id = newEntity.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ثبت پیش‌نویس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش درخواست
        // ============================================================
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] ElmiTermUpdateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<ElmiTerm>()
                    .FirstOrDefaultAsync(e => e.Id == dto.Id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";
                if (isOstad && entity.UserId != currentUser.Id)
                    return Forbid();

                if (!isOstad && entity.UserId.HasValue)
                {
                    if (!await _accessService.CanAccessTargetUserAsync(entity.UserId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                if (entity.ApproveStatus != 0)
                    return BadRequest(new { success = false, message = "درخواست بررسی شده و قابل ویرایش نیست" });

                if (!string.IsNullOrEmpty(dto.AkharinVazeeat)) entity.AkharinVazeeat = dto.AkharinVazeeat;
                if (dto.IsEjeari.HasValue) entity.IsEjeari = dto.IsEjeari;
                if (!string.IsNullOrEmpty(dto.OnvanEjraei)) entity.OnvanEjraei = dto.OnvanEjraei;
                if (dto.FullTime.HasValue) entity.FullTime = dto.FullTime;
                if (dto.TedadSaatMovazafi.HasValue) entity.TedadSaatMovazafi = dto.TedadSaatMovazafi;
                if (dto.TedadVahedMovazafi.HasValue) entity.TedadVahedMovazafi = dto.TedadVahedMovazafi;

                if (dto.File != null)
                {
                    if (!string.IsNullOrEmpty(entity.FilePath))
                        DeleteFile(entity.FilePath);

                    entity.FilePath = await SaveFileAsync(dto.File, entity.Id);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "درخواست با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش درخواست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ تایید/رد درخواست
        // ============================================================
        [HttpPatch("approve")]
        public async Task<IActionResult> Approve([FromBody] ElmiTermApproveDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<ElmiTerm>()
                    .FirstOrDefaultAsync(e => e.Id == dto.Id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (entity.UserId.HasValue)
                {
                    if (!await _accessService.CanAccessTargetUserAsync(entity.UserId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                if (entity.ApproveStatus != 0)
                    return BadRequest(new { success = false, message = "این درخواست قبلاً بررسی شده است" });

                var roleMarkaz = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                entity.ApproveStatus = dto.ApproveStatus;
                entity.ApprovedByUserId = currentUser.Id;
                entity.ApprovedByRoleMarkaz = roleMarkaz;
                entity.ApprovedAt = DateTime.Now;
                entity.ApproveTozihat = dto.Tozihat;

                // ============================================================
                // 🔥 اگر تایید شد، فقط این رکورد فعال شود و بقیه غیرفعال
                // ============================================================
                if (dto.ApproveStatus == 1 && entity.UserId.HasValue)
                {
                    // 1️⃣ این رکورد را فعال کن
                    entity.Vazeeat = true;

                    // 2️⃣ همه رکوردهای دیگر این استاد را غیرفعال کن
                    var otherRecords = await _context.Set<ElmiTerm>()
                        .Where(e => e.UserId == entity.UserId.Value && e.Id != entity.Id)
                        .ToListAsync();

                    foreach (var other in otherRecords)
                    {
                        other.Vazeeat = false;
                    }
                }
                else
                {
                    // اگر رد شد، Vazeeat را false نگه دار (تغییری نمی‌دهیم)
                    entity.Vazeeat = false;
                }

                await _context.SaveChangesAsync();

                var statusText = dto.ApproveStatus == 1 ? "تایید" : "رد";
                return Ok(new
                {
                    success = true,
                    message = $"درخواست با موفقیت {statusText} شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تایید/رد درخواست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ حذف درخواست
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<ElmiTerm>()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (codeRole != 1)
                {
                    if (entity.UserId != currentUser.Id)
                        return Forbid();
                }

                if (!string.IsNullOrEmpty(entity.FilePath))
                    DeleteFile(entity.FilePath);

                _context.Set<ElmiTerm>().Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "درخواست با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف درخواست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ بازگشت درخواست به حالت "در انتظار بررسی"
        // ============================================================
        [HttpPatch("reset-pending/{id}")]
        public async Task<IActionResult> ResetToPending(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<ElmiTerm>()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (entity.UserId.HasValue)
                {
                    if (!await _accessService.CanAccessTargetUserAsync(entity.UserId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                if (entity.ApproveStatus == 0)
                    return BadRequest(new { success = false, message = "این درخواست در حال حاضر در حالت در انتظار بررسی است" });

                entity.ApproveStatus = 0;
                entity.ApprovedByUserId = null;
                entity.ApprovedByRoleMarkaz = null;
                entity.ApprovedAt = null;
                entity.ApproveTozihat = null;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "درخواست با موفقیت به حالت در انتظار بررسی بازگشت"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بازگشت به حالت در انتظار بررسی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 8️⃣ دانلود فایل
        // ============================================================
        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var entity = await _context.Set<ElmiTerm>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null || string.IsNullOrEmpty(entity.FilePath))
                return NotFound(new { message = "فایل یافت نشد" });

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", entity.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "فایل در سرور یافت نشد" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(entity.FilePath);

            var contentType = "application/octet-stream";
            var extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".pdf") contentType = "application/pdf";
            else if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";
            else if (extension == ".png") contentType = "image/png";

            return File(fileBytes, contentType, fileName);
        }
    }
}