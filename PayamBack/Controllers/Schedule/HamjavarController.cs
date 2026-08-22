using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Schedule.Hamjavar;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using PayamBack.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISignatureService _signatureService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;

        public HamjavarController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            ISignatureService signatureService,
            ICurrentUserService currentUserService,
            IAccessService accessService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _signatureService = signatureService;
            _currentUserService = currentUserService;
            _accessService = accessService;
        }

        // ============================================================
        // 🔥 متدهای کمکی (فقط موارد خاص)
        // ============================================================

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

        private bool CanUserCreateForOstad(AppUser currentUser, int targetAppUserId, int codeRole, int? currentMarkazId)
        {
            var targetUser = _userManager.Users
                .Include(u => u.Ostad)
                    .ThenInclude(o => o.Markaz)
                .FirstOrDefault(u => u.Id == targetAppUserId);

            if (targetUser == null)
                return false;

            if (currentUser.Id == targetAppUserId)
                return true;

            if (codeRole == 1 || codeRole == 2)
                return true;

            if (codeRole == 3)
            {
                var hasCreatePermission = _userManager.GetClaimsAsync(currentUser)
                    .Result?.Any(c => c.Type == "Permission" && c.Value == "Hamjavar.CreateMoaven") ?? false;

                if (hasCreatePermission)
                {
                    var currentMarkaz = _context.Markazes.Find(currentMarkazId);
                    return targetUser?.Ostad?.Markaz?.CodeOstan == currentMarkaz?.CodeOstan;
                }
            }

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

        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "uploads", folderName);
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{fileName}";
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
        // 🔥 متدهای محاسبه وضعیت
        // ============================================================

        private static string CalculateAkharinTaghaza(Hamjavar hamjavar)
        {
            if (hamjavar.NazarMoaven != null && hamjavar.NazarMoaven >= 2)
            {
                if (hamjavar.NazarMoaven == 2) return "Taeed";
                if (hamjavar.NazarMoaven == 3) return "Rad";
                if (hamjavar.NazarMoaven == 4) return "Eslah";
            }

            if (hamjavar.NazarKhadamat != null && hamjavar.NazarKhadamat >= 2)
            {
                if (hamjavar.NazarKhadamat == 2) return "Taeed";
                if (hamjavar.NazarKhadamat == 3) return "Rad";
                if (hamjavar.NazarKhadamat == 4) return "Eslah";
            }

            if (hamjavar.NazarRaeis != null && hamjavar.NazarRaeis >= 2)
            {
                if (hamjavar.NazarRaeis == 2) return "Taeed";
                if (hamjavar.NazarRaeis == 3) return "Rad";
                if (hamjavar.NazarRaeis == 4) return "Eslah";
            }

            if (hamjavar.NazarElmi != null && hamjavar.NazarElmi == 2)
            {
                return "Taeed";
            }

            return "PishNevis";
        }

        private static string CalculateAkharinBarrasi(Hamjavar hamjavar)
        {
            if (hamjavar.NazarMoaven != null && hamjavar.NazarMoaven >= 2)
                return "معاونت آموزشی استان";

            if (hamjavar.NazarKhadamat != null && hamjavar.NazarKhadamat >= 2)
                return "خدمات آموزشی استان";

            if (hamjavar.NazarRaeis != null && hamjavar.NazarRaeis >= 2)
                return "رئیس مرکز";

            return "استاد";
        }

        private static string GetStatusDisplayStatic(string status)
        {
            return status switch
            {
                "PishNevis" => "پیش‌نویس",
                "Taeed" => "تایید",
                "Rad" => "رد ❌",
                "Eslah" => "اصلاح ✏️",
                _ => status
            };
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
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var isOstad = currentRole?.Name == "استاد";

                var query = from h in _context.Set<Hamjavar>()
                            join o in _context.Ostads on h.OstadId equals o.Id
                            select new { Hamjavar = h, Ostad = o };

                if (!string.IsNullOrEmpty(termCode))
                    query = query.Where(x => x.Hamjavar.TermCode == termCode);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(x => CalculateAkharinTaghaza(x.Hamjavar) == status);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.Ostad.Naam != null && x.Ostad.Naam.Contains(search)) ||
                        (x.Ostad.NaamKhanevadegi != null && x.Ostad.NaamKhanevadegi.Contains(search)) ||
                        (x.Ostad.CodeOstadi != null && x.Ostad.CodeOstadi.Contains(search)));
                }

                // ============================================================
                // 🔥 محدودیت دسترسی بر اساس نقش
                // ============================================================
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
                else if (codeRole != 1 && codeRole != 2)
                {
                    query = query.Where(x =>
                        x.Hamjavar.NazarElmi != null &&
                        x.Hamjavar.NazarElmi >= 2);
                }

                // ============================================================
                // 🔥 فیلتر بر اساس استان و مرکز (برای ادمین‌ها)
                // ============================================================
                if (ostanId.HasValue && !markazId.HasValue && (codeRole == 1 || codeRole == 2))
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue && (codeRole == 1 || codeRole == 2))
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
                        OstadMarkaz = x.Ostad.Markaz.NaamMarkaz,
                        TermCode = x.Hamjavar.TermCode ?? "",
                        VahedMovazaf = x.Hamjavar.VahedMovazaf ?? 0,
                        TedadVahedMahalKhedmat = x.Hamjavar.TedadVahedMahalKhedmat ?? 0,
                        TedadVahedHamjavar = x.Hamjavar.TedadVahedHamjavar ?? 0,
                        TedadVahedMajazi = x.Hamjavar.TedadVahedMajazi ?? 0,
                        AkharinTaghaza = CalculateAkharinTaghaza(x.Hamjavar),
                        AkharinTaghazaDisplay = GetStatusDisplayStatic(CalculateAkharinTaghaza(x.Hamjavar)),
                        AkharinBarrasi = CalculateAkharinBarrasi(x.Hamjavar),
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
                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var ostad = hamjavar.Ostad;

                string? reshteh = null;
                if (ostad != null)
                {
                    var madrak = await _context.OstadMadraks
                        .Where(m => m.OstadId == ostad.Id && m.PishFarz == true)
                        .FirstOrDefaultAsync();
                    reshteh = madrak?.Reshteh;
                }

                ElmiTerm? elmiTerm = null;
                if (ostad != null)
                {
                    var user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.OstadId == ostad.Id);

                    if (user != null)
                    {
                        elmiTerm = await _context.Set<ElmiTerm>()
                            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.Vazeeat == true);
                    }
                }

                var allFaaliat = await _context.Set<Faaliat>()
                    .Where(f => f.Vazeeat == true)
                    .ToDictionaryAsync(f => f.Id, f => f.Onvan ?? "");

                var akharinTaghaza = CalculateAkharinTaghaza(hamjavar);
                var akharinBarrasi = CalculateAkharinBarrasi(hamjavar);

                // ============================================================
                // 🔥 دریافت امضاها
                // ============================================================
                int? ostadUserId = null;
                if (hamjavar.OstadId > 0)
                {
                    var ostadUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.OstadId == hamjavar.OstadId);
                    ostadUserId = ostadUser?.Id;
                }

                bool isOstadTheCreator = ostadUserId.HasValue &&
                         hamjavar.UserIdSabtKonandeh == ostadUserId.Value;

                var userIds = new List<int>();

                if (isOstadTheCreator && ostadUserId.HasValue)
                    userIds.Add(ostadUserId.Value);

                if (hamjavar.UserIdRaeis.HasValue)
                    userIds.Add(hamjavar.UserIdRaeis.Value);

                if (hamjavar.UserIdKhadamatOstan.HasValue)
                    userIds.Add(hamjavar.UserIdKhadamatOstan.Value);

                if (hamjavar.UserIdApproved.HasValue)
                    userIds.Add(hamjavar.UserIdApproved.Value);

                var signatures = await _signatureService.GetSignaturesByUserIdsAsync(userIds);

                string? raeisFullName = null;
                string? khadamatFullName = null;
                string? moavenFullName = null;

                if (hamjavar.UserIdRaeis.HasValue)
                {
                    var raeisUser = await _userManager.Users
                        .Include(u => u.Karmand)
                        .Include(u => u.Ostad)
                        .Include(u => u.MoshakhasatAdmin)
                        .FirstOrDefaultAsync(u => u.Id == hamjavar.UserIdRaeis.Value);

                    if (raeisUser != null)
                    {
                        if (raeisUser.Karmand != null)
                            raeisFullName = $"{raeisUser.Karmand.Naam} {raeisUser.Karmand.NaameKhanevadeghi}".Trim();
                        else if (raeisUser.Ostad != null)
                            raeisFullName = $"{raeisUser.Ostad.Naam} {raeisUser.Ostad.NaamKhanevadegi}".Trim();
                        else if (raeisUser.MoshakhasatAdmin != null)
                            raeisFullName = $"{raeisUser.MoshakhasatAdmin.Naam} {raeisUser.MoshakhasatAdmin.NaameKhanevadeghi}".Trim();
                    }
                }

                if (hamjavar.UserIdKhadamatOstan.HasValue)
                {
                    var khadamatUser = await _userManager.Users
                        .Include(u => u.Karmand)
                        .Include(u => u.Ostad)
                        .Include(u => u.MoshakhasatAdmin)
                        .FirstOrDefaultAsync(u => u.Id == hamjavar.UserIdKhadamatOstan.Value);

                    if (khadamatUser != null)
                    {
                        if (khadamatUser.Karmand != null)
                            khadamatFullName = $"{khadamatUser.Karmand.Naam} {khadamatUser.Karmand.NaameKhanevadeghi}".Trim();
                        else if (khadamatUser.Ostad != null)
                            khadamatFullName = $"{khadamatUser.Ostad.Naam} {khadamatUser.Ostad.NaamKhanevadegi}".Trim();
                        else if (khadamatUser.MoshakhasatAdmin != null)
                            khadamatFullName = $"{khadamatUser.MoshakhasatAdmin.Naam} {khadamatUser.MoshakhasatAdmin.NaameKhanevadeghi}".Trim();
                    }
                }

                if (hamjavar.UserIdApproved.HasValue)
                {
                    var moavenUser = await _userManager.Users
                        .Include(u => u.Karmand)
                        .Include(u => u.Ostad)
                        .Include(u => u.MoshakhasatAdmin)
                        .FirstOrDefaultAsync(u => u.Id == hamjavar.UserIdApproved.Value);

                    if (moavenUser != null)
                    {
                        if (moavenUser.Karmand != null)
                            moavenFullName = $"{moavenUser.Karmand.Naam} {moavenUser.Karmand.NaameKhanevadeghi}".Trim();
                        else if (moavenUser.Ostad != null)
                            moavenFullName = $"{moavenUser.Ostad.Naam} {moavenUser.Ostad.NaamKhanevadegi}".Trim();
                        else if (moavenUser.MoshakhasatAdmin != null)
                            moavenFullName = $"{moavenUser.MoshakhasatAdmin.Naam} {moavenUser.MoshakhasatAdmin.NaameKhanevadeghi}".Trim();
                    }
                }

                var dto = new HamjavarDetailDto
                {
                    Id = hamjavar.Id,
                    OstadId = hamjavar.OstadId,
                    TermCode = hamjavar.TermCode,
                    VahedMovazaf = hamjavar.VahedMovazaf,
                    TedadVahedMahalKhedmat = hamjavar.TedadVahedMahalKhedmat,
                    TedadVahedHamjavar = hamjavar.TedadVahedHamjavar,
                    TedadVahedMajazi = hamjavar.TedadVahedMajazi,
                    Dalil = hamjavar.Dalil,
                    ShahrZendegi = hamjavar.ShahrZendegi,
                    UploadElmi = hamjavar.UploadElmi,
                    RoleMarkazSabtKonandeh = hamjavar.RoleMarkazSabtKonandeh,

                    OstadName = ostad?.Naam,
                    OstadLastName = ostad?.NaamKhanevadegi,
                    OstadCode = ostad?.CodeOstadi,
                    OstadMarkaz = ostad?.Markaz?.NaamMarkaz,
                    OstadMartabeElmi = ostad?.MartabeElmi,
                    OstadReshteh = reshteh,

                    AkharinVazeeat = elmiTerm?.AkharinVazeeat,
                    IsEjeari = elmiTerm?.IsEjeari,
                    OnvanEjraei = elmiTerm?.OnvanEjraei,
                    FullTime = elmiTerm?.FullTime,
                    TedadSaatMovazafi = elmiTerm?.TedadSaatMovazafi,

                    NazarElmi = hamjavar.NazarElmi,
                    TarikhErsalElmi = hamjavar.TarikhErsalElmi,

                    NazarRaeis = hamjavar.NazarRaeis,
                    TozihatRaeis = hamjavar.TozihatRaeis,
                    RoleMarkazRaeis = hamjavar.RoleMarkazRaeis,
                    TarikhErsalRaeis = hamjavar.TarikhErsalRaeis,
                    RaeisFullName = raeisFullName,
                    UploadRaeis = hamjavar.UploadRaeis,

                    NazarKhadamat = hamjavar.NazarKhadamat,
                    TozihatKhadamat = hamjavar.TozihatKhadamat,
                    RoleMarkazKhadamatOstan = hamjavar.RoleMarkazKhadamatOstan,
                    TarikhErsalKhadamat = hamjavar.TarikhErsalKhadamat,
                    KhadamatFullName = khadamatFullName,
                    UploadKhadamat = hamjavar.UploadKhadamat,

                    NazarMoaven = hamjavar.NazarMoaven,
                    TozihatMoaven = hamjavar.TozihatMoaven,
                    RoleMarkazApproved = hamjavar.RoleMarkazApproved,
                    TarikhErsalMoaven = hamjavar.TarikhErsalMoaven,
                    MoavenFullName = moavenFullName,
                    UploadMoaven = hamjavar.UploadMoaven,

                    AkharinTaghaza = akharinTaghaza,
                    AkharinTaghazaDisplay = GetStatusDisplayStatic(akharinTaghaza),
                    AKharinBarrasi = akharinBarrasi,

                    SignatureOstad = (isOstadTheCreator && ostadUserId.HasValue && signatures.TryGetValue(ostadUserId.Value, out var ostadSig))
                        ? new SignatureDto { Data = ostadSig.Signature, Position = ostadSig.Position } : null,
                    SignatureRaeis = (hamjavar.UserIdRaeis.HasValue && signatures.TryGetValue(hamjavar.UserIdRaeis.Value, out var raeisSig))
                        ? new SignatureDto { Data = raeisSig.Signature, Position = raeisSig.Position } : null,
                    SignatureKhadamat = (hamjavar.UserIdKhadamatOstan.HasValue && signatures.TryGetValue(hamjavar.UserIdKhadamatOstan.Value, out var khadamatSig))
                        ? new SignatureDto { Data = khadamatSig.Signature, Position = khadamatSig.Position } : null,
                    SignatureMoaven = (hamjavar.UserIdApproved.HasValue && signatures.TryGetValue(hamjavar.UserIdApproved.Value, out var approveSig))
                        ? new SignatureDto { Data = approveSig.Signature, Position = approveSig.Position } : null,

                    Hamjavar1s = hamjavar.Hamjavar1s?.Select(d => new Hamjavar1DetailDto
                    {
                        Id = d.Id,
                        MarkazId = d.MarkazId,
                        MarkazName = d.Markaz != null ? d.Markaz.NaamMarkaz ?? "" : "",
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
        public async Task<IActionResult> Create([FromForm] HamjavarCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!CanUserCreateForOstad(currentUser, dto.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var targetUser = await _userManager.Users
                    .Include(u => u.Ostad)
                    .FirstOrDefaultAsync(u => u.Id == dto.OstadId);

                if (targetUser == null || targetUser.OstadId == null)
                    return BadRequest(new { success = false, message = "استاد یافت نشد" });

                var ostadId = targetUser.OstadId.Value;

                var ostad = await _context.Ostads.FindAsync(ostadId);
                if (ostad == null)
                    return BadRequest(new { success = false, message = "استاد یافت نشد" });

                var termExists = await _context.Terms.AnyAsync(t => t.CodeTerm == dto.TermCode);
                if (!termExists)
                    return BadRequest(new { success = false, message = "ترم وارد شده معتبر نیست" });

                var exists = await _context.Set<Hamjavar>()
                    .AnyAsync(h => h.OstadId == ostadId && h.TermCode == dto.TermCode);

                if (exists)
                    return BadRequest(new { success = false, message = "درخواستی برای این استاد در این ترم قبلاً ثبت شده است" });

                List<Hamjavar1CreateDto>? hamjavar1s = null;
                if (!string.IsNullOrEmpty(dto.Hamjavar1sJson))
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        hamjavar1s = JsonSerializer.Deserialize<List<Hamjavar1CreateDto>>(dto.Hamjavar1sJson, options);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ خطا در دسریالایز Hamjavar1s: {ex.Message}");
                        return BadRequest(new { success = false, message = "خطا در پردازش اطلاعات موارد تقاضا" });
                    }
                }

                if (hamjavar1s == null || !hamjavar1s.Any())
                {
                    return BadRequest(new { success = false, message = "حداقل یک مورد تقاضا باید ثبت شود" });
                }

                var roleMarkaz = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                using var transaction = await _context.Database.BeginTransactionAsync();

                string? uploadElmiPath = null;

                try
                {
                    var entity = new Hamjavar
                    {
                        OstadId = ostadId,
                        TermCode = dto.TermCode,
                        UserIdSabtKonandeh = currentUser.Id,
                        RoleMarkazSabtKonandeh = roleMarkaz,
                        VahedMovazaf = dto.VahedMovazaf,
                        TedadVahedMahalKhedmat = dto.TedadVahedMahalKhedmat,
                        TedadVahedHamjavar = dto.TedadVahedHamjavar,
                        TedadVahedMajazi = dto.TedadVahedMajazi,
                        Dalil = dto.Dalil,
                        ShahrZendegi = dto.ShahrZendegi,
                        UploadElmi = null,
                        NazarElmi = 1
                    };

                    await _context.Set<Hamjavar>().AddAsync(entity);
                    await _context.SaveChangesAsync();

                    foreach (var h1Dto in hamjavar1s)
                    {
                        var faaliatIdsString = h1Dto.FaaliatIds != null && h1Dto.FaaliatIds.Any()
                            ? string.Join("|", h1Dto.FaaliatIds)
                            : null;

                        var detail = new Hamjavar1
                        {
                            HamjavarId = entity.Id,
                            UserIdSabtKonandeh = currentUser.Id,
                            RoleMarkazSabtKonandeh = roleMarkaz ?? "نامشخص",
                            MarkazId = h1Dto.MarkazId,
                            InOstan = h1Dto.InOstan ?? true,
                            FaaliatIds = faaliatIdsString,
                            TedadRoozElmi = h1Dto.TedadRoozElmi,
                            TedadRoozRaeis = h1Dto.TedadRoozRaeis,
                            TedadRoozKhadamat = h1Dto.TedadRoozKhadamat,
                            TedadRoozMoaven = h1Dto.TedadRoozMoaven
                        };

                        await _context.Set<Hamjavar1>().AddAsync(detail);
                    }

                    if (dto.UploadElmi != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                        var fileExtension = Path.GetExtension(dto.UploadElmi.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, PDF مجاز است" });
                        }

                        if (dto.UploadElmi.Length > 2 * 1024 * 1024)
                        {
                            return BadRequest(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });
                        }

                        uploadElmiPath = await SaveFileAsync(dto.UploadElmi, "hamjavar");
                        entity.UploadElmi = uploadElmiPath;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "درخواست هم‌جاوری با موفقیت ثبت شد",
                        data = new { id = entity.Id }
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(uploadElmiPath))
                    {
                        DeleteFile(uploadElmiPath);
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ثبت درخواست",
                        error = ex.Message
                    });
                }
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
        // 4️⃣ ویرایش درخواست هم‌جاوری
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] HamjavarUpdateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";
                var currentStatus = CalculateAkharinTaghaza(entity);

                if (isOstad)
                {
                    if (entity.OstadId != currentUser.OstadId)
                        return Forbid();

                    if (currentStatus != "PishNevis")
                        return BadRequest(new { success = false, message = "این درخواست قبلاً تایید شده و قابل ویرایش نیست" });
                }
                else if (codeRole != 1)
                {
                    return Forbid();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                string? uploadElmiPath = null;

                try
                {
                    if (dto.VahedMovazaf.HasValue)
                        entity.VahedMovazaf = dto.VahedMovazaf;

                    if (dto.TedadVahedMahalKhedmat.HasValue)
                        entity.TedadVahedMahalKhedmat = dto.TedadVahedMahalKhedmat;

                    if (dto.TedadVahedHamjavar.HasValue)
                        entity.TedadVahedHamjavar = dto.TedadVahedHamjavar;

                    if (dto.TedadVahedMajazi.HasValue)
                        entity.TedadVahedMajazi = dto.TedadVahedMajazi;

                    if (!string.IsNullOrEmpty(dto.Dalil))
                        entity.Dalil = dto.Dalil;

                    if (!string.IsNullOrEmpty(dto.ShahrZendegi))
                        entity.ShahrZendegi = dto.ShahrZendegi;

                    if (dto.UploadElmi != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                        var fileExtension = Path.GetExtension(dto.UploadElmi.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, PDF مجاز است" });
                        }

                        if (dto.UploadElmi.Length > 2 * 1024 * 1024)
                        {
                            return BadRequest(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });
                        }

                        uploadElmiPath = await SaveFileAsync(dto.UploadElmi, "hamjavar");

                        if (!string.IsNullOrEmpty(entity.UploadElmi))
                        {
                            DeleteFile(entity.UploadElmi);
                        }
                        entity.UploadElmi = uploadElmiPath;
                    }

                    List<Hamjavar1UpdateDto>? hamjavar1s = null;
                    if (!string.IsNullOrEmpty(dto.Hamjavar1sJson))
                    {
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            hamjavar1s = JsonSerializer.Deserialize<List<Hamjavar1UpdateDto>>(dto.Hamjavar1sJson, options);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ خطا در دسریالایز Hamjavar1s: {ex.Message}");
                            return BadRequest(new { success = false, message = "خطا در پردازش اطلاعات موارد تقاضا" });
                        }
                    }

                    if (hamjavar1s != null && hamjavar1s.Any())
                    {
                        var existingIds = entity.Hamjavar1s.Select(d => d.Id).ToList();
                        var updatedIds = hamjavar1s.Where(d => d.Id > 0).Select(d => d.Id).ToList();

                        var idsToDelete = existingIds.Except(updatedIds).ToList();
                        if (idsToDelete.Any())
                        {
                            var itemsToDelete = entity.Hamjavar1s.Where(d => idsToDelete.Contains(d.Id)).ToList();
                            _context.Set<Hamjavar1>().RemoveRange(itemsToDelete);
                        }

                        foreach (var h1Dto in hamjavar1s)
                        {
                            if (h1Dto.Id > 0)
                            {
                                var existing = entity.Hamjavar1s.FirstOrDefault(d => d.Id == h1Dto.Id);
                                if (existing != null)
                                {
                                    if (h1Dto.MarkazId.HasValue)
                                        existing.MarkazId = h1Dto.MarkazId;

                                    if (h1Dto.InOstan.HasValue)
                                        existing.InOstan = h1Dto.InOstan;

                                    if (h1Dto.FaaliatIds != null)
                                    {
                                        existing.FaaliatIds = h1Dto.FaaliatIds.Any()
                                            ? string.Join("|", h1Dto.FaaliatIds)
                                            : null;
                                    }

                                    if (h1Dto.TedadRoozElmi.HasValue)
                                        existing.TedadRoozElmi = h1Dto.TedadRoozElmi;

                                    if (h1Dto.TedadRoozRaeis.HasValue)
                                        existing.TedadRoozRaeis = h1Dto.TedadRoozRaeis;

                                    if (h1Dto.TedadRoozKhadamat.HasValue)
                                        existing.TedadRoozKhadamat = h1Dto.TedadRoozKhadamat;

                                    if (h1Dto.TedadRoozMoaven.HasValue)
                                        existing.TedadRoozMoaven = h1Dto.TedadRoozMoaven;
                                }
                            }
                            else
                            {
                                var faaliatIdsString = h1Dto.FaaliatIds != null && h1Dto.FaaliatIds.Any()
                                    ? string.Join("|", h1Dto.FaaliatIds)
                                    : null;

                                var newDetail = new Hamjavar1
                                {
                                    HamjavarId = entity.Id,
                                    UserIdSabtKonandeh = currentUser.Id,
                                    RoleMarkazSabtKonandeh = GetRoleMarkazDisplay(currentRole, currentMarkaz),
                                    MarkazId = h1Dto.MarkazId ?? 0,
                                    InOstan = h1Dto.InOstan ?? true,
                                    FaaliatIds = faaliatIdsString,
                                    TedadRoozElmi = h1Dto.TedadRoozElmi,
                                    TedadRoozRaeis = h1Dto.TedadRoozRaeis,
                                    TedadRoozKhadamat = h1Dto.TedadRoozKhadamat,
                                    TedadRoozMoaven = h1Dto.TedadRoozMoaven
                                };

                                await _context.Set<Hamjavar1>().AddAsync(newDetail);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "درخواست با موفقیت ویرایش شد",
                        data = new { id = entity.Id }
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(uploadElmiPath))
                    {
                        DeleteFile(uploadElmiPath);
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ویرایش درخواست",
                        error = ex.Message
                    });
                }
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
        // 5️⃣ تایید نهایی توسط استاد
        // ============================================================
        [HttpPatch("confirm-submit-by-ostad/{id}")]
        public async Task<IActionResult> ConfirmSubmitByOstad(int id, [FromBody] HamjavarConfirmDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";

                if (!isOstad || entity.OstadId != currentUser.OstadId)
                    return Forbid();

                var currentStatus = CalculateAkharinTaghaza(entity);
                if (currentStatus != "PishNevis")
                    return BadRequest(new { success = false, message = "این درخواست قبلاً تایید شده است" });

                entity.NazarElmi = dto.Nazar;
                entity.TarikhErsalElmi = DateTime.Now;

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
        // 6️⃣ بررسی توسط رئیس مرکز
        // ============================================================
        [HttpPatch("review-raeis")]
        public async Task<IActionResult> ReviewByRaeis([FromForm] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await _accessService.CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.NazarElmi == null || hamjavar.NazarElmi != 2)
                {
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید شود (NazarElmi=2)" });
                }

                if (hamjavar.NazarRaeis != null && hamjavar.NazarRaeis >= 2)
                {
                    return BadRequest(new { success = false, message = "نظر رئیس مرکز قبلاً ثبت شده است" });
                }

                if (hamjavar.NazarKhadamat != null && hamjavar.NazarKhadamat >= 2)
                {
                    return BadRequest(new { success = false, message = "نظر خدمات آموزشی قبلاً ثبت شده است" });
                }

                if (hamjavar.NazarMoaven != null && hamjavar.NazarMoaven >= 2)
                {
                    return BadRequest(new { success = false, message = "نظر معاونت آموزشی قبلاً ثبت شده است" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                string? uploadRaeisPath = null;

                try
                {
                    if (dto.UploadFile != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                        var fileExtension = Path.GetExtension(dto.UploadFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, PDF مجاز است" });
                        }

                        if (dto.UploadFile.Length > 2 * 1024 * 1024)
                        {
                            return BadRequest(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });
                        }

                        uploadRaeisPath = await SaveFileAsync(dto.UploadFile, "hamjavar");
                    }

                    if (dto.TedadRoozList != null && dto.TedadRoozList.Any())
                    {
                        foreach (var item in dto.TedadRoozList)
                        {
                            var detail = hamjavar.Hamjavar1s.FirstOrDefault(d => d.Id == item.Id);
                            if (detail != null)
                            {
                                detail.TedadRoozRaeis = item.TedadRooz;
                            }
                        }
                    }

                    hamjavar.NazarRaeis = dto.Nazar;
                    hamjavar.TozihatRaeis = dto.Tozihat;
                    hamjavar.TarikhErsalRaeis = DateTime.Now;
                    hamjavar.UserIdRaeis = currentUser.Id;
                    hamjavar.RoleMarkazRaeis = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                    if (!string.IsNullOrEmpty(uploadRaeisPath))
                    {
                        if (!string.IsNullOrEmpty(hamjavar.UploadRaeis))
                        {
                            DeleteFile(hamjavar.UploadRaeis);
                        }
                        hamjavar.UploadRaeis = uploadRaeisPath;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "نظر رئیس مرکز با موفقیت ثبت شد"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(uploadRaeisPath))
                    {
                        DeleteFile(uploadRaeisPath);
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ثبت نظر رئیس مرکز",
                        error = ex.Message
                    });
                }
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
        // 7️⃣ بررسی توسط خدمات آموزشی استان
        // ============================================================
        [HttpPatch("review-khadamat")]
        public async Task<IActionResult> ReviewByKhadamat([FromForm] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await _accessService.CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.NazarElmi == null || hamjavar.NazarElmi != 2)
                {
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید شود (NazarElmi=2)" });
                }

                if (hamjavar.NazarKhadamat != null && hamjavar.NazarKhadamat >= 2)
                {
                    return BadRequest(new { success = false, message = "نظر خدمات آموزشی قبلاً ثبت شده است" });
                }

                if (hamjavar.NazarMoaven != null && hamjavar.NazarMoaven >= 2)
                {
                    return BadRequest(new { success = false, message = "نظر معاونت آموزشی قبلاً ثبت شده است" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                string? uploadKhadamatPath = null;

                try
                {
                    if (dto.UploadFile != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                        var fileExtension = Path.GetExtension(dto.UploadFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, PDF مجاز است" });
                        }

                        if (dto.UploadFile.Length > 2 * 1024 * 1024)
                        {
                            return BadRequest(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });
                        }

                        uploadKhadamatPath = await SaveFileAsync(dto.UploadFile, "hamjavar");
                    }

                    if (dto.TedadRoozList != null && dto.TedadRoozList.Any())
                    {
                        foreach (var item in dto.TedadRoozList)
                        {
                            var detail = hamjavar.Hamjavar1s.FirstOrDefault(d => d.Id == item.Id);
                            if (detail != null)
                            {
                                detail.TedadRoozKhadamat = item.TedadRooz;
                            }
                        }
                    }

                    hamjavar.NazarKhadamat = dto.Nazar;
                    hamjavar.TozihatKhadamat = dto.Tozihat;
                    hamjavar.TarikhErsalKhadamat = DateTime.Now;
                    hamjavar.UserIdKhadamatOstan = currentUser.Id;
                    hamjavar.RoleMarkazKhadamatOstan = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                    if (!string.IsNullOrEmpty(uploadKhadamatPath))
                    {
                        if (!string.IsNullOrEmpty(hamjavar.UploadKhadamat))
                        {
                            DeleteFile(hamjavar.UploadKhadamat);
                        }
                        hamjavar.UploadKhadamat = uploadKhadamatPath;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "نظر خدمات آموزشی استان با موفقیت ثبت شد"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(uploadKhadamatPath))
                    {
                        DeleteFile(uploadKhadamatPath);
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ثبت نظر خدمات آموزشی",
                        error = ex.Message
                    });
                }
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
        // 8️⃣ بررسی نهایی توسط معاونت آموزشی استان
        // ============================================================
        [HttpPatch("review-moaven")]
        public async Task<IActionResult> ReviewByMoaven([FromForm] HamjavarReviewDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var hamjavar = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == dto.HamjavarId);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                if (!await _accessService.CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                if (hamjavar.NazarElmi == null || hamjavar.NazarElmi != 2)
                {
                    return BadRequest(new { success = false, message = "این درخواست باید ابتدا توسط استاد تایید شود (NazarElmi=2)" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                string? uploadMoavenPath = null;

                try
                {
                    if (dto.UploadFile != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                        var fileExtension = Path.GetExtension(dto.UploadFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, PDF مجاز است" });
                        }

                        if (dto.UploadFile.Length > 2 * 1024 * 1024)
                        {
                            return BadRequest(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });
                        }

                        uploadMoavenPath = await SaveFileAsync(dto.UploadFile, "hamjavar");
                    }

                    if (dto.TedadRoozList != null && dto.TedadRoozList.Any())
                    {
                        foreach (var item in dto.TedadRoozList)
                        {
                            var detail = hamjavar.Hamjavar1s.FirstOrDefault(d => d.Id == item.Id);
                            if (detail != null)
                            {
                                detail.TedadRoozMoaven = item.TedadRooz;
                            }
                        }
                    }

                    hamjavar.NazarMoaven = dto.Nazar;
                    hamjavar.TozihatMoaven = dto.Tozihat;
                    hamjavar.TarikhErsalMoaven = DateTime.Now;
                    hamjavar.UserIdApproved = currentUser.Id;
                    hamjavar.RoleMarkazApproved = GetRoleMarkazDisplay(currentRole, currentMarkaz);

                    if (!string.IsNullOrEmpty(uploadMoavenPath))
                    {
                        if (!string.IsNullOrEmpty(hamjavar.UploadMoaven))
                        {
                            DeleteFile(hamjavar.UploadMoaven);
                        }
                        hamjavar.UploadMoaven = uploadMoavenPath;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "نظر معاونت آموزشی استان با موفقیت ثبت شد"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(uploadMoavenPath))
                    {
                        DeleteFile(uploadMoavenPath);
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ثبت نظر معاونت آموزشی",
                        error = ex.Message
                    });
                }
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
        // 9️⃣ حذف کامل درخواست هم‌جاوری
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.Set<Hamjavar>()
                    .Include(h => h.Hamjavar1s)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    if (entity.OstadId != currentUser.OstadId)
                        return Forbid();

                    if (entity.NazarElmi != null && entity.NazarElmi > 1)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "درخواست تایید شده و قابل حذف نیست. فقط در حالت پیش‌نویس (NazarElmi=0 یا 1) قابل حذف است"
                        });
                    }
                }
                else if (codeRole != 1)
                {
                    return Forbid();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (!string.IsNullOrEmpty(entity.UploadElmi))
                        DeleteFile(entity.UploadElmi);

                    if (!string.IsNullOrEmpty(entity.UploadRaeis))
                        DeleteFile(entity.UploadRaeis);

                    if (!string.IsNullOrEmpty(entity.UploadKhadamat))
                        DeleteFile(entity.UploadKhadamat);

                    if (!string.IsNullOrEmpty(entity.UploadMoaven))
                        DeleteFile(entity.UploadMoaven);

                    if (entity.Hamjavar1s != null && entity.Hamjavar1s.Any())
                    {
                        _context.Set<Hamjavar1>().RemoveRange(entity.Hamjavar1s);
                    }

                    _context.Set<Hamjavar>().Remove(entity);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "درخواست و تمام زیرمجموعه‌های آن با موفقیت حذف شد"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در حذف درخواست",
                        error = ex.Message
                    });
                }
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
        // 🔟 دریافت Hamjavar1 های یک درخواست
        // ============================================================
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetailsByHamjavarId(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var hamjavar = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hamjavar == null)
                    return NotFound(new { success = false, message = "درخواست یافت نشد" });

                var isOstad = currentRole?.Name == "استاد";

                if (isOstad && hamjavar.OstadId != currentUser.OstadId)
                    return Forbid();

                if (!isOstad && !await _accessService.CanAccessTargetOstadAsync(hamjavar.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var details = await _context.Set<Hamjavar1>()
                    .Include(h1 => h1.Markaz)
                    .Where(h1 => h1.HamjavarId == id)
                    .Select(d => new Hamjavar1DetailDto
                    {
                        Id = d.Id,
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

        // ============================================================
        // 1️⃣1️⃣ دانلود فایل‌های مستندات هم‌جاوری
        // ============================================================
        [HttpGet("download/{id}/{fileType}")]
        [Authorize]
        public async Task<IActionResult> GetDownloadFile(int id, string fileType)
        {
            try
            {
                var entity = await _context.Set<Hamjavar>()
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (entity == null)
                    return NotFound(new { message = "درخواست یافت نشد" });

                string? filePath = fileType.ToLower() switch
                {
                    "elmi" => entity.UploadElmi,
                    "raeis" => entity.UploadRaeis,
                    "khadamat" => entity.UploadKhadamat,
                    "moaven" => entity.UploadMoaven,
                    _ => null
                };

                if (string.IsNullOrEmpty(filePath))
                    return NotFound(new { message = "فایل مورد نظر یافت نشد" });

                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", filePath.TrimStart('/'));
                if (!System.IO.File.Exists(physicalPath))
                    return NotFound(new { message = "فایل در سرور یافت نشد" });

                var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                var fileName = Path.GetFileName(filePath);

                var contentType = "application/octet-stream";
                var extension = Path.GetExtension(fileName).ToLower();
                if (extension == ".pdf") contentType = "application/pdf";
                else if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";
                else if (extension == ".png") contentType = "image/png";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دانلود فایل", error = ex.Message });
            }
        }
    }
}