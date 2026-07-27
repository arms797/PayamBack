using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PayamBack.Data;
using PayamBack.DTOs.Core.Ostad;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.Security.Claims;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class OstadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public OstadController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // 🔥 متد کمکی برای دریافت اطلاعات کاربر فعلی و نقش فعال
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
        // 🔥 متد کمکی برای بررسی دسترسی به مرکز هدف
        // ============================================================
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
        // 🔥 متد کمکی برای گرفتن مراکز قابل دسترس
        // ============================================================
        private async Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1)
            {
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null) return new List<int>();

            if (codeRole == 2)
            {
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true && (m.Level == 2 || m.Level == 3))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 3)
            {
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true &&
                        (m.Level == 3 && m.CodeOstan == currentMarkaz.CodeOstan) ||
                        (m.Level == 4 && m.CodeOstan == currentMarkaz.CodeOstan))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 4)
            {
                return new List<int> { currentMarkaz.Id };
            }

            return new List<int>();
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
            [FromQuery] bool? vazeeat = null,
            [FromQuery] bool? vazeeatMovaghat = null)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و مراکز قابل دسترس
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                // ============================================================
                // 1️⃣ ساخت کوئری پایه با Join به AppUser
                // ============================================================
                var query = from o in _context.Ostads
                            join u in _context.Users on o.Id equals u.OstadId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where o.MarkazId.HasValue && accessibleMarkazIds.Contains(o.MarkazId.Value)
                            select new { Ostad = o, User = u };

                // ============================================================
                // 2️⃣ فیلتر بر اساس جستجو
                // ============================================================
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

                // ============================================================
                // 3️⃣ فیلتر بر اساس رشته تحصیلی
                // ============================================================
                if (!string.IsNullOrEmpty(reshteh))
                {
                    query = query.Where(x =>
                        _context.OstadMadraks
                            .Any(m => m.OstadId == x.Ostad.Id && m.Reshteh != null && m.Reshteh.Contains(reshteh)));
                }

                // ============================================================
                // 4️⃣ فیلتر بر اساس استان و مرکز
                // ============================================================
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

                // ============================================================
                // 5️⃣ فیلتر بر اساس نوع همکاری
                // ============================================================
                if (noeHamkari.HasValue)
                {
                    query = query.Where(x => x.Ostad.NoeHamkari == (NoeHamkariEnum)noeHamkari.Value);
                }

                // ============================================================
                // 6️⃣ فیلتر بر اساس وضعیت (Vazeeat و VazeeatMovaghat)
                // ============================================================
                if (vazeeat.HasValue)
                {
                    if (vazeeat == true)
                    {
                        query = query.Where(x => x.User != null &&
                            (x.User.Vazeeyat == true && x.User.VazeeyatMovaghat == true));
                    }
                    else
                    {
                        query = query.Where(x => x.User != null &&
                            (x.User.Vazeeyat == vazeeat.Value || x.User.VazeeyatMovaghat == vazeeat.Value));
                    }
                }
                else
                {
                    query = query.Where(x => x.User == null || x.User.Vazeeyat == true);
                }

                // ============================================================
                // 7️⃣ محاسبه تعداد کل
                // ============================================================
                var totalCount = await query.CountAsync();

                // ============================================================
                // 8️⃣ صفحه‌بندی
                // ============================================================
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
                    //Vazeeat = ostad.Vazeeat ?? true,
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
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // بررسی کد استادی تکراری
                var exists = await _context.Ostads
                    .AnyAsync(o => o.CodeOstadi == dto.CodeOstadi);

                if (exists)
                    return BadRequest(new { success = false, message = "کد استادی قبلاً ثبت شده است" });

                var existingUser = await _userManager.FindByNameAsync(dto.CodeOstadi);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "کد استادی قبلاً به عنوان نام کاربری ثبت شده است" });

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
                    ShomareMelli = dto.ShomareMelli,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
                    Mobile2 = dto.Mobile2,
                    MartabeElmi = dto.MartabeElmi,
                    SazmanMarboote = dto.SazmanMarboote,
                    MahalEshteghal = dto.MahalEshteghal,
                    Emza = dto.Emza,
                    //Vazeeat = true,
                    NoeHamkari = (NoeHamkariEnum?)dto.NoeHamkari,
                    NoeBimeh = dto.NoeBimeh,
                    ShomarehBimeh = dto.ShomarehBimeh
                };

                await _context.Ostads.AddAsync(ostad);
                await _context.SaveChangesAsync();

                var user = new AppUser
                {
                    UserName = dto.CodeOstadi,
                    Email = dto.Email,
                    OstadId = ostad.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = true
                };

                var password = dto.ShomareMelli + "aA";
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _context.Ostads.Remove(ostad);
                    await _context.SaveChangesAsync();

                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد کاربر",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                if (!string.IsNullOrEmpty(dto.RoleName))
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
        // 4️⃣ آپلود گروهی اساتید از Excel
        // ============================================================
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // فقط ادمین سامانه (CodeRole=1) یا ادمین سازمان (CodeRole=2) اجازه آپلود دارند
                if (codeRole > 2)
                    return Forbid();

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "فایل انتخاب نشده است" });

                if (!file.FileName.EndsWith(".xlsx"))
                    return BadRequest(new { success = false, message = "فرمت فایل باید xlsx باشد" });

                var ostads = new List<Ostad>();
                var users = new List<AppUser>();
                var errors = new List<string>();
                var batchSize = 200;
                var rowCount = 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCountTotal = worksheet.Dimension?.Rows ?? 0;

                if (rowCountTotal < 2)
                    return BadRequest(new { success = false, message = "فایل خالی است" });

                // دریافت مراکز قابل دسترس برای اعتبارسنجی
                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                for (int row = 2; row <= rowCountTotal; row++)
                {
                    try
                    {
                        var codeOstadi = worksheet.Cells[row, 1].Text?.Trim();
                        var naam = worksheet.Cells[row, 2].Text?.Trim();
                        var naamKhanevadegi = worksheet.Cells[row, 3].Text?.Trim();
                        var shomareMelli = worksheet.Cells[row, 4].Text?.Trim();
                        var email = worksheet.Cells[row, 5].Text?.Trim();
                        var mobile = worksheet.Cells[row, 6].Text?.Trim();
                        var markazId = int.TryParse(worksheet.Cells[row, 7].Text?.Trim(), out int mId) ? mId : (int?)null;
                        var noeHamkari = int.TryParse(worksheet.Cells[row, 8].Text?.Trim(), out int nHamkari) ? nHamkari : 3;

                        if (string.IsNullOrEmpty(codeOstadi) || string.IsNullOrEmpty(shomareMelli))
                        {
                            errors.Add($"ردیف {row}: کد استادی و کد ملی الزامی است");
                            continue;
                        }

                        // بررسی دسترسی به مرکز
                        if (markazId.HasValue && !accessibleMarkazIds.Contains(markazId.Value))
                        {
                            errors.Add($"ردیف {row}: شما دسترسی به مرکز {markazId} را ندارید");
                            continue;
                        }

                        // بررسی تکراری در لیست فعلی
                        if (ostads.Any(o => o.CodeOstadi == codeOstadi))
                        {
                            errors.Add($"ردیف {row}: کد استادی {codeOstadi} تکراری است");
                            continue;
                        }

                        // بررسی تکراری در دیتابیس
                        var exists = await _context.Ostads.AnyAsync(o => o.CodeOstadi == codeOstadi);
                        if (exists)
                        {
                            errors.Add($"ردیف {row}: کد استادی {codeOstadi} قبلاً ثبت شده است");
                            continue;
                        }

                        var userExists = await _userManager.FindByNameAsync(codeOstadi);
                        if (userExists != null)
                        {
                            errors.Add($"ردیف {row}: کد استادی {codeOstadi} قبلاً به عنوان نام کاربری ثبت شده است");
                            continue;
                        }

                        var ostad = new Ostad
                        {
                            CodeOstadi = codeOstadi,
                            Naam = naam,
                            NaamKhanevadegi = naamKhanevadegi,
                            ShomareMelli = shomareMelli,
                            Email = email,
                            Mobile = mobile,
                            MarkazId = markazId,
                            NoeHamkari = (NoeHamkariEnum?)noeHamkari,
                            //Vazeeat = true
                        };

                        ostads.Add(ostad);

                        var user = new AppUser
                        {
                            UserName = codeOstadi,
                            Email = email,
                            OstadId = ostad.Id,
                            Vazeeyat = true,
                            VazeeyatMovaghat = true
                        };
                        users.Add(user);

                        rowCount++;

                        if (rowCount % batchSize == 0)
                        {
                            await SaveOstadBatch(ostads, users);
                            ostads.Clear();
                            users.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"ردیف {row}: خطا در پردازش - {ex.Message}");
                    }
                }

                if (ostads.Any())
                {
                    await SaveOstadBatch(ostads, users);
                }

                return Ok(new
                {
                    success = true,
                    message = $"تعداد {rowCount} استاد با موفقیت ثبت شد",
                    errors = errors.Any() ? errors : null,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در آپلود فایل", error = ex.Message });
            }
        }

        private async Task SaveOstadBatch(List<Ostad> ostads, List<AppUser> users)
        {
            await _context.Ostads.AddRangeAsync(ostads);
            await _context.SaveChangesAsync();

            foreach (var user in users)
            {
                var ostad = ostads.FirstOrDefault(o => o.Id == user.OstadId);
                var password = ostad?.ShomareMelli ?? "123456";
                await _userManager.CreateAsync(user, password + "aA");
            }
        }

        // ============================================================
        // 5️⃣ ویرایش استاد
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OstadUpdateDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var ostad = await _context.Ostads
                    .Include(o => o.Markaz)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                if (!await CanAccessTargetMarkazAsync(ostad.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                ostad.Naam = dto.Naam ?? ostad.Naam;
                ostad.NaamKhanevadegi = dto.NaamKhanevadegi ?? ostad.NaamKhanevadegi;
                ostad.MarkazId = dto.MarkazId ?? ostad.MarkazId;
                ostad.MarkazAsliId = dto.MarkazAsliId ?? ostad.MarkazAsliId;
                ostad.Jens = dto.Jens ?? ostad.Jens;
                ostad.Email = dto.Email ?? ostad.Email;
                ostad.Mobile = dto.Mobile ?? ostad.Mobile;
                ostad.Mobile2 = dto.Mobile2 ?? ostad.Mobile2;
                //ostad.Vazeeat = dto.Vazeeat ?? ostad.Vazeeat;
                ostad.NoeHamkari = dto.NoeHamkari ?? ostad.NoeHamkari;
                ostad.NoeBimeh = dto.NoeBimeh ?? ostad.NoeBimeh;
                ostad.ShomarehBimeh = dto.ShomarehBimeh ?? ostad.ShomarehBimeh;

                await _context.SaveChangesAsync();

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.OstadId == id);

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
        // 6️⃣ حذف استاد (فقط ادمین سامانه)
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                var ostad = await _context.Ostads.FindAsync(id);
                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.OstadId == id);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Ostads.Remove(ostad);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "استاد و کاربر مربوطه حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف استاد", error = ex.Message });
            }
        }

        // ============================================================
        // 7️⃣ تغییر وضعیت گروهی اساتید (فعال/غیرفعال) - فقط ادمین سامانه
        // ============================================================
        [HttpPatch("toggle")]
        public async Task<IActionResult> Toggle([FromBody] List<ToggleOstadStatusItemDto> items)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // فقط ادمین سامانه (CodeRole == 1) اجازه دارد
                if (codeRole != 1)
                    return Forbid();

                // ============================================================
                // 1️⃣ اعتبارسنجی ورودی
                // ============================================================
                if (items == null || !items.Any())
                    return BadRequest(new { success = false, message = "لیست اساتید خالی است" });

                var duplicateUserIds = items
                    .GroupBy(x => x.UserId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateUserIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "شناسه کاربران تکراری است",
                        duplicateUserIds
                    });
                }

                var invalidItems = items
                    .Where(x => x.Vazeeyat == null && x.VazeeyatMovaghat == null)
                    .Select(x => x.UserId)
                    .ToList();

                if (invalidItems.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "برای هر کاربر حداقل یکی از وضعیت‌ها باید مقدار داشته باشد",
                        invalidUserIds = invalidItems
                    });
                }

                // ============================================================
                // 2️⃣ دریافت کاربران هدف
                // ============================================================
                var userIds = items.Select(x => x.UserId).Distinct().ToList();

                var targetUsers = await _userManager.Users
                    .Include(u => u.Ostad)
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                if (!targetUsers.Any())
                    return NotFound(new { success = false, message = "هیچ کاربری یافت نشد" });

                var validItems = new List<(ToggleOstadStatusItemDto item, AppUser user)>();
                var notFoundUsers = new List<int>();
                var notOstadUsers = new List<int>();

                foreach (var item in items)
                {
                    var user = targetUsers.FirstOrDefault(u => u.Id == item.UserId);
                    if (user == null)
                    {
                        notFoundUsers.Add(item.UserId);
                        continue;
                    }

                    if (user.OstadId == null)
                    {
                        notOstadUsers.Add(item.UserId);
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
                        notOstad = notOstadUsers.Any() ? notOstadUsers : null
                    });
                }

                // ============================================================
                // 3️⃣ تغییر وضعیت
                // ============================================================
                var updatedUsers = new List<object>();
                var failedUsers = new List<object>();

                foreach (var (item, user) in validItems)
                {
                    try
                    {
                        if (item.Vazeeyat.HasValue)
                        {
                            user.Vazeeyat = item.Vazeeyat.Value;
                        }

                        if (item.VazeeyatMovaghat.HasValue)
                        {
                            user.VazeeyatMovaghat = item.VazeeyatMovaghat.Value;
                        }

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            updatedUsers.Add(new
                            {
                                userId = user.Id,
                                userName = user.UserName,
                                ostadId = user.OstadId,
                                vazeeyat = user.Vazeeyat,
                                vazeeyatMovaghat = user.VazeeyatMovaghat
                            });
                        }
                        else
                        {
                            failedUsers.Add(new
                            {
                                userId = user.Id,
                                userName = user.UserName,
                                errors = result.Errors.Select(e => e.Description)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        failedUsers.Add(new
                        {
                            userId = user.Id,
                            userName = user.UserName,
                            error = ex.Message
                        });
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
                        notOstad = notOstadUsers.Any() ? notOstadUsers : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت اساتید",
                    error = ex.Message
                });
            }
        }
    }

    // ============================================================
    // 🔥 DTO برای تغییر وضعیت گروهی اساتید
    // ============================================================
    public class ToggleOstadStatusItemDto
    {
        public int UserId { get; set; }
        public bool? Vazeeyat { get; set; }
        public bool? VazeeyatMovaghat { get; set; }
    }
}