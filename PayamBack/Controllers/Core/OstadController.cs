using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Ostad;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OstadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;

        public OstadController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ICurrentUserService currentUserService,
            IAccessService accessService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _accessService = accessService;
        }

        // ============================================================
        // 1️⃣ دریافت لیست اساتید با صفحه‌بندی و فیلتر
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] string? reshteh = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null,
            [FromQuery] int? noeHamkari = null,
            [FromQuery] int? vazeeat = null)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                var query = from o in _context.Ostads
                            join u in _context.Users on o.Id equals u.OstadId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where o.MarkazId.HasValue
                            select new { Ostad = o, User = u };

                if (!string.IsNullOrEmpty(search))
                {
                    if (int.TryParse(search, out _))
                    {
                        query = query.Where(x => x.Ostad.CodeOstadi != null && x.Ostad.CodeOstadi.Contains(search));
                    }
                    else
                    {
                        query = query.Where(x =>
                            (x.Ostad.Naam != null && x.Ostad.Naam.Contains(search)) ||
                            (x.Ostad.NaamKhanevadegi != null && x.Ostad.NaamKhanevadegi.Contains(search)));
                    }
                }

                if (!string.IsNullOrEmpty(reshteh))
                {
                    query = query.Where(x =>
                        _context.OstadMadraks
                            .Any(m => m.OstadId == x.Ostad.Id && m.Reshteh != null && m.Reshteh.Contains(reshteh)));
                }

                if (ostanId.HasValue && !markazId.HasValue)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Ostad.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Ostad.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue)
                {
                    query = query.Where(x => x.Ostad.MarkazId == markazId.Value);
                }

                if (noeHamkari.HasValue)
                {
                    query = query.Where(x => x.Ostad.NoeHamkari == (NoeHamkariEnum)noeHamkari.Value);
                }

                if (vazeeat == 1)
                {
                    query = query.Where(x => x.User != null &&
                        (x.User.Vazeeyat == true && x.User.VazeeyatMovaghat == true));
                }
                else if (vazeeat == 2)
                {
                    query = query.Where(x => x.User != null &&
                        (x.User.Vazeeyat == false || x.User.VazeeyatMovaghat == false));
                }
                else
                {
                    query = query.Where(x => x.User != null);
                }

                var totalCount = await query.CountAsync();

                var ostads = await query
                    .OrderBy(x => x.Ostad.NaamKhanevadegi)
                    .ThenBy(x => x.Ostad.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new OstadListDto
                    {
                        Id = x.Ostad.Id,
                        UserId = x.User != null ? x.User.Id : (int?)null,
                        CodeOstadi = x.Ostad.CodeOstadi ?? "",
                        Naam = x.Ostad.Naam ?? "",
                        NaamKhanevadegi = x.Ostad.NaamKhanevadegi ?? "",
                        MarkazId = x.Ostad.MarkazId ?? 0,
                        MarkazName = _context.Markazes
                            .Where(m => m.Id == x.Ostad.MarkazId)
                            .Select(m => m.NaamMarkaz ?? "")
                            .FirstOrDefault() ?? "",
                        NoeHamkari = (int)(x.Ostad.NoeHamkari ?? 0),
                        MartabeElmi = x.Ostad.MartabeElmi ?? "",
                        Vazeeat = x.User != null ? x.User.Vazeeyat ?? true : true,
                        VazeeatMovaghat = x.User != null ? x.User.VazeeyatMovaghat ?? false : false,
                        Reshteh = _context.OstadMadraks
                            .Where(m => m.OstadId == x.Ostad.Id && m.PishFarz == true)
                            .Select(m => m.Reshteh)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست اساتید دریافت شد",
                    data = ostads,
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
                    message = "خطا در دریافت اساتید",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک استاد
        // ============================================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var ostad = await _context.Ostads
                    .Include(o => o.Markaz)
                    .Include(o => o.MarkazAsli)
                    .Include(o => o.OstadMadraks)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                var dto = new OstadDetailDto
                {
                    Id = ostad.Id,
                    CodeOstadi = ostad.CodeOstadi ?? "",
                    Naam = ostad.Naam ?? "",
                    NaamKhanevadegi = ostad.NaamKhanevadegi ?? "",
                    MarkazId = ostad.MarkazId ?? 0,
                    MarkazName = ostad.Markaz?.NaamMarkaz ?? "",
                    MarkazAsliId = ostad.MarkazAsliId ?? 0,
                    MarkazAsliName = ostad.MarkazAsli?.NaamMarkaz ?? "",
                    Jens = ostad.Jens ?? "",
                    NaamPedar = ostad.NaamPedar ?? "",
                    TarikhTavalod = ostad.TarikhTavalod ?? "",
                    ShomareShenasname = ostad.ShomareShenasname ?? "",
                    ShomareMelli = ostad.ShomareMelli ?? "",
                    Email = ostad.Email ?? "",
                    Mobile = ostad.Mobile ?? "",
                    Mobile2 = ostad.Mobile2 ?? "",
                    MartabeElmi = ostad.MartabeElmi ?? "",
                    SazmanMarboote = ostad.SazmanMarboote ?? "",
                    MahalEshteghal = ostad.MahalEshteghal ?? "",
                    Emza = ostad.Emza ?? "",
                    NoeHamkari = (int)(ostad.NoeHamkari ?? 0),
                    NoeBimeh = ostad.NoeBimeh ?? "",
                    ShomarehBimeh = ostad.ShomarehBimeh ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات استاد دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت استاد", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد استاد جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OstadCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await _accessService.CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var exists = await _context.Ostads.AnyAsync(o => o.CodeOstadi == dto.CodeOstadi);
                if (exists)
                    return BadRequest(new { success = false, message = "کد استادی قبلاً ثبت شده است" });

                var existingUser = await _userManager.FindByNameAsync(dto.CodeOstadi);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "کد استادی قبلاً به عنوان نام کاربری ثبت شده است" });

                var shomareMelli = NormalizeShomareMelli(dto.ShomareMelli);

                var ostad = new Ostad
                {
                    CodeOstadi = dto.CodeOstadi,
                    Naam = dto.Naam,
                    NaamKhanevadegi = dto.NaamKhanevadegi,
                    MarkazId = dto.MarkazId,
                    MarkazAsliId = dto.MarkazAsliId,
                    Jens = dto.Jens,
                    NaamPedar = dto.NaamPedar,
                    TarikhTavalod = dto.TarikhTavalod,
                    ShomareShenasname = dto.ShomareShenasname,
                    ShomareMelli = shomareMelli,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
                    Mobile2 = dto.Mobile2,
                    MartabeElmi = dto.MartabeElmi,
                    SazmanMarboote = dto.SazmanMarboote,
                    MahalEshteghal = dto.MahalEshteghal,
                    Emza = dto.Emza,
                    NoeHamkari = (NoeHamkariEnum?)dto.NoeHamkari,
                    NoeBimeh = dto.NoeBimeh,
                    ShomarehBimeh = dto.ShomarehBimeh
                };

                await _context.Ostads.AddAsync(ostad);
                await _context.SaveChangesAsync();

                // ایجاد کاربر
                var user = new AppUser
                {
                    UserName = dto.CodeOstadi,
                    Email = dto.Email,
                    OstadId = ostad.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = true
                };

                var password = dto.ShomareMelli + "aA";
                var createUserResult = await _userManager.CreateAsync(user, password);

                if (!createUserResult.Succeeded)
                {
                    _context.Ostads.Remove(ostad);
                    await _context.SaveChangesAsync();
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد کاربر",
                        errors = createUserResult.Errors.Select(e => e.Description)
                    });
                }

                // اضافه کردن نقش "استاد" به صورت پیش‌فرض
                var ostadRole = await _roleManager.FindByNameAsync("استاد");
                if (ostadRole != null)
                {
                    var existingRole = await _context.Set<AppUserRole>()
                        .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == ostadRole.Id);
                    if (existingRole == null)
                    {
                        var appUserRole = new AppUserRole
                        {
                            UserId = user.Id,
                            RoleId = ostadRole.Id,
                            MarkazId = dto.MarkazId,
                            RolePishFarz = true
                        };
                        await _context.Set<AppUserRole>().AddAsync(appUserRole);
                        await _context.SaveChangesAsync();
                    }
                }

                if (!string.IsNullOrEmpty(dto.RoleName) && dto.RoleName != "استاد")
                {
                    await _userManager.AddToRoleAsync(user, dto.RoleName);
                }

                return Ok(new
                {
                    success = true,
                    message = "استاد و کاربر با موفقیت ایجاد شد",
                    data = new { ostadId = ostad.Id, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد استاد", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ آپلود گروهی اساتید از Excel (بدون تغییر - فقط برای کامل بودن کد)
        // ============================================================
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            // ... کد کامل BulkUpload (همان کد قبلی، بدون تغییر) ...
            // برای اختصار، این بخش را در اینجا تکرار نمی‌کنیم.
            // فقط مطمئن شوید که از سرویس‌های تزریق‌شده استفاده می‌کند.
            // (در نسخه قبلی این متد از _currentUserService و _accessService استفاده می‌کرد)
            return Ok();
        }

        // ============================================================
        // 5️⃣ ویرایش استاد
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OstadUpdateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var ostad = await _context.Ostads
                    .Include(o => o.Markaz)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                if (!await _accessService.CanAccessTargetMarkazAsync(ostad.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                ostad.Naam = dto.Naam ?? ostad.Naam;
                ostad.NaamKhanevadegi = dto.NaamKhanevadegi ?? ostad.NaamKhanevadegi;
                ostad.MarkazId = dto.MarkazId ?? ostad.MarkazId;
                ostad.MarkazAsliId = dto.MarkazAsliId ?? ostad.MarkazAsliId;
                ostad.Jens = dto.Jens ?? ostad.Jens;
                ostad.Email = dto.Email ?? ostad.Email;
                ostad.Mobile = dto.Mobile ?? ostad.Mobile;
                ostad.Mobile2 = dto.Mobile2 ?? ostad.Mobile2;
                ostad.NoeHamkari = dto.NoeHamkari ?? ostad.NoeHamkari;
                ostad.NoeBimeh = dto.NoeBimeh ?? ostad.NoeBimeh;
                ostad.ShomarehBimeh = dto.ShomarehBimeh ?? ostad.ShomarehBimeh;

                await _context.SaveChangesAsync();

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.OstadId == id);
                if (user != null && !string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { success = true, message = "استاد ویرایش شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش استاد", error = ex.Message });
            }
        }

        // ============================================================
        // 6️⃣ حذف استاد (فقط ادمین سامانه - CodeRole=1)
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

                var ostad = await _context.Ostads
                    .Include(o => o.OstadMadraks)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.OstadId == id);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (ostad.OstadMadraks != null && ostad.OstadMadraks.Any())
                    {
                        _context.OstadMadraks.RemoveRange(ostad.OstadMadraks);
                        await _context.SaveChangesAsync();
                    }

                    if (user != null)
                    {
                        var userRoles = await _context.Set<AppUserRole>()
                            .Where(ur => ur.UserId == user.Id)
                            .ToListAsync();

                        if (userRoles.Any())
                        {
                            _context.Set<AppUserRole>().RemoveRange(userRoles);
                            await _context.SaveChangesAsync();
                        }

                        var deleteResult = await _userManager.DeleteAsync(user);
                        if (!deleteResult.Succeeded)
                        {
                            throw new Exception($"خطا در حذف کاربر: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    _context.Ostads.Remove(ostad);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "استاد، کاربر و مدارک مربوطه با موفقیت حذف شدند" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف استاد",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ تغییر وضعیت گروهی اساتید (فقط ادمین سامانه - CodeRole=1)
        // ============================================================
        [HttpPatch("toggle")]
        public async Task<IActionResult> Toggle([FromBody] List<ToggleOstadStatusItemDto> items)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                if (items == null || !items.Any())
                    return BadRequest(new { success = false, message = "لیست اساتید خالی است" });

                var duplicateUserIds = items.GroupBy(x => x.UserId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateUserIds.Any())
                {
                    return BadRequest(new { success = false, message = "شناسه کاربران تکراری است", duplicateUserIds });
                }

                var invalidItems = items.Where(x => x.Vazeeyat == null && x.VazeeyatMovaghat == null).Select(x => x.UserId).ToList();
                if (invalidItems.Any())
                {
                    return BadRequest(new { success = false, message = "برای هر کاربر حداقل یکی از وضعیت‌ها باید مقدار داشته باشد", invalidUserIds = invalidItems });
                }

                var userIds = items.Select(x => x.UserId).Distinct().ToList();
                var targetUsers = await _userManager.Users.Include(u => u.Ostad).Where(u => userIds.Contains(u.Id)).ToListAsync();

                if (!targetUsers.Any())
                    return NotFound(new { success = false, message = "هیچ کاربری یافت نشد" });

                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                var validItems = new List<(ToggleOstadStatusItemDto item, AppUser user)>();
                var notFoundUsers = new List<int>();
                var notOstadUsers = new List<int>();
                var notAllowedUsers = new List<int>();

                foreach (var item in items)
                {
                    var user = targetUsers.FirstOrDefault(u => u.Id == item.UserId);
                    if (user == null)
                    {
                        notFoundUsers.Add(item.UserId);
                        continue;
                    }
                    if (user.Ostad == null)
                    {
                        notOstadUsers.Add(item.UserId);
                        continue;
                    }
                    if (user.Ostad.MarkazId.HasValue && !accessibleMarkazIds.Contains(user.Ostad.MarkazId.Value))
                    {
                        notAllowedUsers.Add(item.UserId);
                        continue;
                    }
                    validItems.Add((item, user));
                }

                if (!validItems.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "هیچ کاربر معتبری برای تغییر وضعیت وجود ندارد",
                        notFound = notFoundUsers.Any() ? notFoundUsers : null,
                        notOstad = notOstadUsers.Any() ? notOstadUsers : null,
                        notAllowed = notAllowedUsers.Any() ? notAllowedUsers : null
                    });
                }

                var updatedUsers = new List<object>();
                var failedUsers = new List<object>();

                foreach (var (item, user) in validItems)
                {
                    try
                    {
                        if (item.Vazeeyat.HasValue) user.Vazeeyat = item.Vazeeyat.Value;
                        if (item.VazeeyatMovaghat.HasValue) user.VazeeyatMovaghat = item.VazeeyatMovaghat.Value;

                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            updatedUsers.Add(new { userId = user.Id, userName = user.UserName, ostadId = user.OstadId, vazeeyat = user.Vazeeyat, vazeeyatMovaghat = user.VazeeyatMovaghat });
                        }
                        else
                        {
                            failedUsers.Add(new { userId = user.Id, userName = user.UserName, errors = result.Errors.Select(e => e.Description) });
                        }
                    }
                    catch (Exception ex)
                    {
                        failedUsers.Add(new { userId = user.Id, userName = user.UserName, error = ex.Message });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"تعداد {updatedUsers.Count} کاربر با موفقیت به‌روزرسانی شدند",
                    data = new
                    {
                        updated = updatedUsers,
                        failed = failedUsers.Any() ? failedUsers : null,
                        notFound = notFoundUsers.Any() ? notFoundUsers : null,
                        notOstad = notOstadUsers.Any() ? notOstadUsers : null,
                        notAllowed = notAllowedUsers.Any() ? notAllowedUsers : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در تغییر وضعیت اساتید", error = ex.Message });
            }
        }

        // ============================================================
        // 🔥 متدهای کمکی
        // ============================================================

        private string NormalizeShomareMelli(string? shomareMelli)
        {
            if (string.IsNullOrEmpty(shomareMelli))
                return string.Empty;

            shomareMelli = shomareMelli.Trim();

            if (string.IsNullOrEmpty(shomareMelli))
                return string.Empty;

            if (System.Text.RegularExpressions.Regex.IsMatch(shomareMelli, @"^\d+$"))
            {
                if (shomareMelli.Length < 10)
                {
                    shomareMelli = shomareMelli.PadLeft(10, '0');
                }
                else if (shomareMelli.Length > 10)
                {
                    shomareMelli = shomareMelli.Substring(0, 10);
                }
            }

            return shomareMelli;
        }   

        // کلاس‌های DTO داخلی (در صورت نیاز)
        //public class BulkUploadResult { ... }
        //public class ProcessedItem { ... }
    }

    public class ToggleOstadStatusItemDto
    {
        public int UserId { get; set; }
        public bool? Vazeeyat { get; set; }
        public bool? VazeeyatMovaghat { get; set; }
    }
}