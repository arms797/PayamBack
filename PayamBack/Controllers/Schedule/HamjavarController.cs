using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Schedule.Hamjavar;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using System.Security.Claims;

namespace PayamBack.Controllers.Schedule
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HamjavarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public HamjavarController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // 🔥 متدهای کمکی
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

        private async Task<List<string>> GetUserPermissionsAsync(AppUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            return claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();
        }

        private async Task<bool> HasPermissionAsync(AppUser user, string permission)
        {
            var permissions = await GetUserPermissionsAsync(user);
            return permissions.Contains(permission);
        }

        private async Task<bool> CanAccessTargetOstadAsync(int ostadId, int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1 || codeRole == 2) return true;

            var ostad = await _context.Ostads
                .Include(o => o.Markaz)
                .FirstOrDefaultAsync(o => o.Id == ostadId);

            if (ostad == null || ostad.MarkazId == null) return false;

            var targetMarkaz = await _context.Markazes.FindAsync(ostad.MarkazId.Value);
            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (targetMarkaz == null || currentMarkaz == null) return false;

            if (codeRole == 3)
                return targetMarkaz.CodeOstan == currentMarkaz.CodeOstan;

            if (codeRole == 4)
                return targetMarkaz.Id == currentMarkaz.Id;

            return false;
        }

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

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "PishNevis" => "پیش‌نویس",
                "TaeedSabt" => "تایید استاد",
                "DarEntezarRaeis" => "در انتظار بررسی رئیس مرکز",
                "TaeedRaeis" => "تایید رئیس مرکز",
                "RadRaeis" => "رد رئیس مرکز",
                "DarEntezarKhadamat" => "در انتظار بررسی خدمات آموزشی استان",
                "TaeedKhadamat" => "تایید خدمات آموزشی استان",
                "RadKhadamat" => "رد خدمات آموزشی استان",
                "DarEntezarMoaven" => "در انتظار بررسی معاونت آموزشی استان",
                "TaeedNahaei" => "تایید نهایی",
                "RadNahaei" => "رد نهایی",
                _ => status
            };
        }

        private bool CanUserCreateForOstad(AppUser user, int ostadId, int codeRole, int? currentMarkazId)
        {
            // 1️⃣ اگر کاربر خودش استاد است
            if (user.OstadId == ostadId)
                return true;

            // 2️⃣ ادمین سامانه (کد 1) می‌تواند برای هر استادی ایجاد کند
            if (codeRole == 1)
                return true;

            // 3️⃣ سازمان مرکزی (کد 2) می‌تواند برای هر استادی ایجاد کند
            if (codeRole == 2)
                return true;

            // 4️⃣ معاون آموزشی استان با مجوز Hamjavar.CreateMoaven
            if (codeRole == 3)
            {
                var hasCreatePermission = _userManager.GetClaimsAsync(user)
                    .Result?.Any(c => c.Type == "Permission" && c.Value == "Hamjavar.CreateMoaven") ?? false;

                if (hasCreatePermission)
                {
                    var ostad = _context.Ostads.Include(o => o.Markaz).FirstOrDefault(o => o.Id == ostadId);
                    var currentMarkaz = _context.Markazes.Find(currentMarkazId);
                    return ostad?.Markaz?.CodeOstan == currentMarkaz?.CodeOstan;
                }
            }

            return false;
        }

        // ============================================================
        // 🔥 تبدیل FaaliatIds
        // ============================================================
        private string ConvertFaaliatIdsToString(List<int> faaliatIds)
        {
            return string.Join("|", faaliatIds);
        }

        private List<int> ConvertFaaliatIdsToList(string? faaliatIds)
        {
            if (string.IsNullOrEmpty(faaliatIds))
                return new List<int>();

            return faaliatIds
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }

        // ============================================================
        // 1️⃣ دریافت لیست درخواست‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? termCode = null,
            [FromQuery] string? status = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var isOstad = currentRole?.Name == "استاد";

                var query = from h in _context.Set<Hamjavar>()
                            join o in _context.Ostads on h.OstadId equals o.Id
                            select new { Hamjavar = h, Ostad = o };

                if (!string.IsNullOrEmpty(termCode))
                    query = query.Where(x => x.Hamjavar.TermCode == termCode);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(x => x.Hamjavar.AkharinTaghaza == status);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.Ostad.Naam != null && x.Ostad.Naam.Contains(search)) ||
                        (x.Ostad.NaamKhanevadegi != null && x.Ostad.NaamKhanevadegi.Contains(search)) ||
                        (x.Ostad.CodeOstadi != null && x.Ostad.CodeOstadi.Contains(search)));
                }

                // محدودیت بر اساس نقش
                if (isOstad)
                {
                    query = query.Where(x => x.Hamjavar.OstadId == currentUser.OstadId);
                }
                else if (codeRole == 3 && currentMarkaz != null)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == currentMarkaz.CodeOstan)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (codeRole == 4 && currentMarkaz != null)
                {
                    query = query.Where(x => x.Ostad.MarkazId == currentMarkaz.Id);
                }

                if (ostanId.HasValue && !markazId.HasValue && !isOstad)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue && !isOstad)
                {
                    query = query.Where(x => x.Ostad.MarkazId == markazId.Value);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.Hamjavar.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new HamjavarListDto
                    {
                        Id = x.Hamjavar.Id,
                        OstadId = x.Hamjavar.OstadId,
                        OstadName = $"{x.Ostad.Naam} {x.Ostad.NaamKhanevadegi}",
                        OstadCode = x.Ostad.CodeOstadi ?? "",
                        TermCode = x.Hamjavar.TermCode ?? "",
                        VahedMovazaf = x.Hamjavar.VahedMovazaf ?? 0,
                        TedadVahedMahalKhedmat = x.Hamjavar.TedadVahedMahalKhedmat ?? 0,
                        TedadVahedHamjavar = x.Hamjavar.TedadVahedHamjavar ?? 0,
                        TedadVahedMajazi = x.Hamjavar.TedadVahedMajazi ?? 0,
                        AkharinTaghaza = x.Hamjavar.AkharinTaghaza ?? "PishNevis",
                        AkharinTaghazaDisplay = GetStatusDisplay(x.Hamjavar.AkharinTaghaza ?? "PishNevis"),
                        KharinBarrasi = x.Hamjavar.AKharinBarrasi ?? "",
                        HasHamjavar1s = _context.Set<Hamjavar1>().Any(h1 => h1.HamjavarId == x.Hamjavar.Id),
                        CreatedAt = DateTime.Now
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست درخواست‌های هم‌جاوری دریافت شد",
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
                var item = await _context.Set<Hamjavar>()
                    .Include(h => h.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Include(h => h.Term)
                    .Include(h => h.Hamjavar1s)
                        .ThenInclude(h1 => h1.Markaz)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (item == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var isOstad = currentRole?.Name == "استاد";
                var permissions = await GetUserPermissionsAsync(currentUser);
                var isMoaven = permissions.Contains("Hamjavar.ReviewMoaven");

                // بررسی دسترسی
                if (isOstad && item.OstadId != currentUser.OstadId)
                    return Forbid();

                if (!isOstad && !await CanAccessTargetOstadAsync(item.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // ثبت تاریخ دریافت
                if (codeRole == 4 && !item.TarikhDaryaftRaeis.HasValue)
                {
                    item.TarikhDaryaftRaeis = DateTime.Now;
                }
                else if (codeRole == 3 && !item.TarikhDaryaftKhadamat.HasValue)
                {
                    item.TarikhDaryaftKhadamat = DateTime.Now;
                }
                else if (isMoaven && !item.TarikhDaryaftMoaven.HasValue)
                {
                    item.TarikhDaryaftMoaven = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var allFaaliat = await _context.Set<Faaliat>()
                    .Where(f => f.Vazeeat == true)
                    .ToDictionaryAsync(f => f.Id, f => f.Onvan ?? "");

                var dto = new HamjavarDetailDto
                {
                    Id = item.Id,
                    OstadId = item.OstadId,
                    OstadName = $"{item.Ostad?.Naam} {item.Ostad?.NaamKhanevadegi}",
                    OstadCode = item.Ostad?.CodeOstadi ?? "",
                    OstadMarkaz = item.Ostad?.Markaz?.NaamMarkaz ?? "",
                    TermCode = item.TermCode ?? "",
                    TermName = item.Term?.OnvanTerm ?? "",
                    VahedMovazaf = item.VahedMovazaf ?? 0,
                    TedadVahedMahalKhedmat = item.TedadVahedMahalKhedmat ?? 0,
                    TedadVahedHamjavar = item.TedadVahedHamjavar ?? 0,
                    TedadVahedMajazi = item.TedadVahedMajazi ?? 0,
                    Dalil = item.Dalil ?? "",
                    ShahrZendegi = item.ShahrZendegi ?? "",
                    UploadElmi = item.UploadElmi ?? "",
                    AmaliatElmi = item.AmaliatElmi,
                    NazarElmi = item.NazarElmi ?? "",
                    TarikhErsalElmi = item.TarikhErsalElmi,
                    TarikhDaryaftRaeis = item.TarikhDaryaftRaeis,
                    TozihatRaeis = item.TozihatRaeis ?? "",
                    UploadRaeis = item.UploadRaeis ?? "",
                    AmaliatRaeis = item.AmaliatRaeis,
                    NazarRaeis = item.NazarRaeis ?? "",
                    TarikhErsalRaeis = item.TarikhErsalRaeis,
                    UserIdRaeis = item.UserIdRaeis,
                    RoleMarkazRaeis = item.RoleMarkazRaeis ?? "",
                    TarikhDaryaftKhadamat = item.TarikhDaryaftKhadamat,
                    TozihatKhadamat = item.TozihatKhadamat ?? "",
                    UploadKhadamat = item.UploadKhadamat ?? "",
                    AmaliatKhadamat = item.AmaliatKhadamat,
                    NazarKhadamat = item.NazarKhadamat ?? "",
                    TarikhErsalKhadamat = item.TarikhErsalKhadamat,
                    UserIdKhadamatOstan = item.UserIdKhadamatOstan,
                    RoleMarkazKhadamatOstan = item.RoleMarkazKhadamatOstan ?? "",
                    TarikhDaryaftMoaven = item.TarikhDaryaftMoaven,
                    TozihatMoaven = item.TozihatMoaven ?? "",
                    UploadMoaven = item.UploadMoaven ?? "",
                    AmaliatMoaven = item.AmaliatMoaven,
                    NazarMoaven = item.NazarMoaven ?? "",
                    TarikhErsalMoaven = item.TarikhErsalMoaven,
                    UserIdApproved = item.UserIdApproved,
                    RoleMarkazApproved = item.RoleMarkazApproved ?? "",
                    KharinBarrasi = item.AKharinBarrasi ?? "",
                    AkharinTaghaza = item.AkharinTaghaza ?? "PishNevis",
                    AkharinTaghazaDisplay = GetStatusDisplay(item.AkharinTaghaza ?? "PishNevis"),
                    Hamjavar1s = item.Hamjavar1s?.Select(d => new Hamjavar1DetailDto
                    {
                        Id = d.Id,
                        HamjavarId = d.HamjavarId,
                        MarkazId = d.MarkazId,
                        MarkazName = d.Markaz?.NaamMarkaz ?? "",
                        InOstan = d.InOstan ?? false,
                        FaaliatIds = ConvertFaaliatIdsToList(d.FaaliatIds),
                        FaaliatNames = ConvertFaaliatIdsToList(d.FaaliatIds)
                            .Where(id => allFaaliat.ContainsKey(id))
                            .Select(id => allFaaliat[id])
                            .ToList(),
                        TedadRoozElmi = d.TedadRoozElmi,
                        TedadRoozRaeis = d.TedadRoozRaeis,
                        TedadRoozKhadamat = d.TedadRoozKhadamat,
                        TedadRoozMoaven = d.TedadRoozMoaven
                    }).ToList() ?? new()
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
        public async Task<IActionResult> Create([FromBody] HamjavarCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // بررسی دسترسی برای ایجاد
                if (!CanUserCreateForOstad(currentUser, dto.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var ostad = await _context.Ostads.FindAsync(dto.OstadId);
                if (ostad == null)
                    return BadRequest(new { success = false, message = "استاد یافت نشد" });

                var termExists = await _context.Terms.AnyAsync(t => t.CodeTerm == dto.TermCode);
                if (!termExists)
                    return BadRequest(new { success = false, message = "ترم وارد شده معتبر نیست" });

                var exists = await _context.Set<Hamjavar>()
                    .AnyAsync(h => h.OstadId == dto.OstadId && h.TermCode == dto.TermCode);

                if (exists)
                    return BadRequest(new { success = false, message = "درخواستی برای این استاد در این ترم قبلاً ثبت شده است" });

                var roleMarkaz = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                var entity = new Hamjavar
                {
                    OstadId = dto.OstadId,
                    TermCode = dto.TermCode,
                    UserIdSabtKonandeh = currentUser.Id,
                    RoleMarkazSabtKonandeh = roleMarkaz,
                    VahedMovazaf = dto.VahedMovazaf,
                    TedadVahedMahalKhedmat = dto.TedadVahedMahalKhedmat,
                    TedadVahedHamjavar = dto.TedadVahedHamjavar,
                    TedadVahedMajazi = dto.TedadVahedMajazi,
                    Dalil = dto.Dalil,
                    ShahrZendegi = dto.ShahrZendegi,
                    UploadElmi = dto.UploadElmi,
                    AkharinTaghaza = "PishNevis",
                    AKharinBarrasi = "پیش‌نویس"
                };

                await _context.Set<Hamjavar>().AddAsync(entity);
                await _context.SaveChangesAsync();

                // ایجاد Hamjavar1 ها
                if (dto.Hamjavar1s != null && dto.Hamjavar1s.Any())
                {
                    foreach (var h1Dto in dto.Hamjavar1s)
                    {
                        var faaliatIdsString = ConvertFaaliatIdsToString(h1Dto.FaaliatIds);

                        var detail = new Hamjavar1
                        {
                            HamjavarId = entity.Id,
                            UserIdSabtKonandeh = currentUser.Id,
                            RoleMarkazSabtKonandeh = roleMarkaz,
                            MarkazId = h1Dto.MarkazId,
                            InOstan = h1Dto.InOstan,
                            FaaliatIds = faaliatIdsString,
                            TedadRoozElmi = h1Dto.TedadRoozElmi,
                            TedadRoozRaeis = h1Dto.TedadRoozRaeis,
                            TedadRoozKhadamat = h1Dto.TedadRoozKhadamat,
                            TedadRoozMoaven = h1Dto.TedadRoozMoaven
                        };

                        await _context.Set<Hamjavar1>().AddAsync(detail);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "درخواست هم‌جاوری با موفقیت ثبت شد",
                    data = new { id = entity.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ثبت درخواست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ تایید نهایی توسط استاد
        // ============================================================
        [HttpPatch("confirm-submit-by-ostad/{id}")]
        public async Task<IActionResult> ConfirmSubmitByOstad(int id, [FromBody] HamjavarConfirmDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";

                if (!isOstad || entity.OstadId != currentUser.OstadId)
                    return Forbid();

                if (entity.AkharinTaghaza != "PishNevis")
                    return BadRequest(new { success = false, message = "این درخواست قبلاً تایید شده است" });

                entity.TarikhErsalElmi = DateTime.Now;
                entity.NazarElmi = dto.Nazar;
                entity.AmaliatElmi = 1;
                entity.AkharinTaghaza = "TaeedSabt";
                entity.AKharinBarrasi = "تایید نهایی توسط استاد";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "درخواست با موفقیت تایید نهایی شد و برای سایرین قابل مشاهده است"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تایید نهایی درخواست",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ بررسی توسط رئیس مرکز
        // ============================================================
        [HttpPatch("review-raeis")]
        public async Task<IActionResult> ReviewByRaeis([FromBody] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // بررسی مجوز رئیس مرکز
                if (!await HasPermissionAsync(currentUser, "Hamjavar.ReviewRaeis"))
                    return Forbid();

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.AkharinTaghaza != "TaeedSabt")
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید نهایی شود" });

                if (hamjavar.AmaliatRaeis == 1)
                    return BadRequest(new { success = false, message = "نظر رئیس مرکز قبلاً ثبت شده است" });

                // به‌روزرسانی Hamjavar1
                for (int i = 0; i < hamjavar.Hamjavar1s.Count; i++)
                {
                    var detail = hamjavar.Hamjavar1s.ElementAt(i);
                    var tedadRooz = dto.TedadRoozList != null && dto.TedadRoozList.Count > i
                        ? dto.TedadRoozList[i]
                        : (int?)null;

                    detail.TedadRoozRaeis = tedadRooz;
                }

                hamjavar.NazarRaeis = dto.Nazar;
                hamjavar.AmaliatRaeis = 1;
                hamjavar.TarikhErsalRaeis = DateTime.Now;
                hamjavar.UserIdRaeis = currentUser.Id;
                hamjavar.RoleMarkazRaeis = GetRoleMarkazDisplay(currentRole, currentMarkaz);
                hamjavar.AKharinBarrasi = "نظر رئیس مرکز ثبت شد";
                hamjavar.AkharinTaghaza = "TaeedRaeis";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "نظر رئیس مرکز با موفقیت ثبت شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بررسی رئیس مرکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ بررسی توسط خدمات آموزشی استان
        // ============================================================
        [HttpPatch("review-khadamat")]
        public async Task<IActionResult> ReviewByKhadamat([FromBody] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // بررسی مجوز خدمات آموزشی استان
                if (!await HasPermissionAsync(currentUser, "Hamjavar.ReviewKhadamat"))
                    return Forbid();

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.AkharinTaghaza != "TaeedSabt")
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید نهایی شود" });

                if (hamjavar.AmaliatKhadamat == 1)
                    return BadRequest(new { success = false, message = "نظر خدمات آموزشی استان قبلاً ثبت شده است" });

                // به‌روزرسانی Hamjavar1
                for (int i = 0; i < hamjavar.Hamjavar1s.Count; i++)
                {
                    var detail = hamjavar.Hamjavar1s.ElementAt(i);
                    var tedadRooz = dto.TedadRoozList != null && dto.TedadRoozList.Count > i
                        ? dto.TedadRoozList[i]
                        : (int?)null;

                    detail.TedadRoozKhadamat = tedadRooz;
                }

                hamjavar.NazarKhadamat = dto.Nazar;
                hamjavar.AmaliatKhadamat = 1;
                hamjavar.TarikhErsalKhadamat = DateTime.Now;
                hamjavar.UserIdKhadamatOstan = currentUser.Id;
                hamjavar.RoleMarkazKhadamatOstan = GetRoleMarkazDisplay(currentRole, currentMarkaz);
                hamjavar.AKharinBarrasi = "نظر خدمات آموزشی استان ثبت شد";
                hamjavar.AkharinTaghaza = "TaeedKhadamat";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "نظر خدمات آموزشی استان با موفقیت ثبت شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بررسی خدمات آموزشی استان",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ بررسی نهایی توسط معاونت آموزشی استان
        // ============================================================
        [HttpPatch("review-moaven")]
        public async Task<IActionResult> ReviewByMoaven([FromBody] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // بررسی مجوز معاونت آموزشی استان
                if (!await HasPermissionAsync(currentUser, "Hamjavar.ReviewMoaven"))
                    return Forbid();

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.AkharinTaghaza != "TaeedSabt")
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید نهایی شود" });

                // به‌روزرسانی Hamjavar1
                for (int i = 0; i < hamjavar.Hamjavar1s.Count; i++)
                {
                    var detail = hamjavar.Hamjavar1s.ElementAt(i);
                    var tedadRooz = dto.TedadRoozList != null && dto.TedadRoozList.Count > i
                        ? dto.TedadRoozList[i]
                        : (int?)null;

                    detail.TedadRoozMoaven = tedadRooz;
                }

                hamjavar.NazarMoaven = dto.Nazar;
                hamjavar.AmaliatMoaven = 1;
                hamjavar.TarikhErsalMoaven = DateTime.Now;
                hamjavar.UserIdApproved = currentUser.Id;
                hamjavar.RoleMarkazApproved = GetRoleMarkazDisplay(currentRole, currentMarkaz);
                hamjavar.AKharinBarrasi = "نظر معاونت آموزشی استان ثبت شد";
                hamjavar.AkharinTaghaza = "TaeedNahaei";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "نظر معاونت آموزشی استان با موفقیت ثبت شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بررسی معاونت آموزشی استان",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 8️⃣ حذف درخواست
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                // فقط خود استاد (در حالت پیش‌نویس) یا ادمین می‌تواند حذف کند
                var isOstad = currentRole?.Name == "استاد";
                if (isOstad)
                {
                    if (entity.OstadId != currentUser.OstadId)
                        return Forbid();

                    if (entity.AkharinTaghaza != "PishNevis")
                        return BadRequest(new { success = false, message = "درخواست تایید شده و قابل حذف نیست" });
                }
                else if (codeRole != 1)
                {
                    return Forbid();
                }

                // حذف Hamjavar1 ها
                var details = await _context.Set<Hamjavar1>()
                    .Where(h1 => h1.HamjavarId == id)
                    .ToListAsync();

                if (details.Any())
                {
                    _context.Set<Hamjavar1>().RemoveRange(details);
                }

                _context.Set<Hamjavar>().Remove(entity);
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
        // 9️⃣ بازگشت به مرحله قبل (فقط ادمین سامانه)
        // ============================================================
       /* [HttpPatch("rollback/{id}")]
        public async Task<IActionResult> Rollback(int id, [FromBody] HamjavarRollbackDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // فقط ادمین سامانه (کد 1) اجازه بازگشت دارد
                if (codeRole != 1)
                    return Forbid();

                var entity = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                switch (entity.AkharinTaghaza)
                {
                    case "TaeedRaeis":
                    case "RadRaeis":
                        entity.AkharinTaghaza = "TaeedSabt";
                        entity.AKharinBarrasi = "بازگشت به تایید استاد";
                        break;
                    case "TaeedKhadamat":
                    case "RadKhadamat":
                        entity.AkharinTaghaza = "TaeedRaeis";
                        entity.AKharinBarrasi = "بازگشت به تایید رئیس مرکز";
                        break;
                    case "TaeedNahaei":
                    case "RadNahaei":
                        entity.AkharinTaghaza = "TaeedKhadamat";
                        entity.AKharinBarrasi = "بازگشت به تایید خدمات آموزشی استان";
                        break;
                    default:
                        return BadRequest(new { success = false, message = "وضعیت فعلی قابل بازگشت نیست" });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "درخواست با موفقیت به مرحله قبل بازگشت"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بازگشت به مرحله قبل",
                    error = ex.Message
                });
            }
        }
       */

        // ============================================================
        // 🔟 دریافت Hamjavar1 های یک درخواست
        // ============================================================
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetailsByHamjavarId(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var hamjavar = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";

                // بررسی دسترسی
                if (isOstad && hamjavar.OstadId != currentUser.OstadId)
                    return Forbid();

                if (!isOstad && !await CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var details = await _context.Set<Hamjavar1>()
                    .Include(h1 => h1.Markaz)
                    .Where(h1 => h1.HamjavarId == id)
                    .Select(d => new Hamjavar1DetailDto
                    {
                        Id = d.Id,
                        HamjavarId = d.HamjavarId,
                        MarkazId = d.MarkazId,
                        MarkazName = d.Markaz != null ? d.Markaz.NaamMarkaz ?? "" : "",
                        InOstan = d.InOstan ?? true,
                        FaaliatIds = ConvertFaaliatIdsToList(d.FaaliatIds),
                        TedadRoozElmi = d.TedadRoozElmi,
                        TedadRoozRaeis = d.TedadRoozRaeis,
                        TedadRoozKhadamat = d.TedadRoozKhadamat,
                        TedadRoozMoaven = d.TedadRoozMoaven
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "جزئیات درخواست دریافت شد",
                    data = details
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت جزئیات",
                    error = ex.Message
                });
            }
        }
    }
}