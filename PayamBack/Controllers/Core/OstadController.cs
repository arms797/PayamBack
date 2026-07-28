using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Ostad;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.Security.Claims;
using ClosedXML.Excel;  

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
            if (codeRole == 1 || codeRole == 2) return true;

            var targetMarkaz = await _context.Markazes.FindAsync(targetMarkazId);
            if (targetMarkaz == null) return false;

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null) return false;

            if (codeRole == 3)
                return targetMarkaz.CodeOstan == currentMarkaz.CodeOstan;

            if (codeRole == 4)
                return targetMarkaz.Id == currentMarkaz.Id;

            return false;
        }

        // ============================================================
        // 🔥 متد کمکی برای گرفتن مراکز قابل دسترس
        // ============================================================
        private async Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1 || codeRole == 2)
            {
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null) return new List<int>();

            if (codeRole == 3)
            {
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true &&
                        m.CodeOstan == currentMarkaz.CodeOstan)
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
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                var query = from o in _context.Ostads
                            join u in _context.Users on o.Id equals u.OstadId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where o.MarkazId.HasValue && accessibleMarkazIds.Contains(o.MarkazId.Value)
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

                // ============================================================
                // 🔥 فیلتر بر اساس وضعیت از AppUser (نه از Ostad)
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
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var exists = await _context.Ostads.AnyAsync(o => o.CodeOstadi == dto.CodeOstadi);
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
                    NoeHamkari = (NoeHamkariEnum?)dto.NoeHamkari,
                    NoeBimeh = dto.NoeBimeh,
                    ShomarehBimeh = dto.ShomarehBimeh
                };

                await _context.Ostads.AddAsync(ostad);
                await _context.SaveChangesAsync();

                // ============================================================
                // 🔥 ایجاد کاربر
                // ============================================================
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

                // ============================================================
                // 🔥 اضافه کردن نقش "استاد" به صورت پیش‌فرض
                // ============================================================
                var ostadRole = await _roleManager.FindByNameAsync("استاد");
                if (ostadRole != null)
                {
                    // ============================================================
                    // 🔥 بررسی اینکه کاربر قبلاً این نقش را ندارد
                    // ============================================================
                    var existingRole = await _context.Set<AppUserRole>()
                        .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == ostadRole.Id);
                    if (existingRole == null)
                    {
                        // اضافه کردن نقش به کاربر
                        await _userManager.AddToRoleAsync(user, ostadRole.Name);

                        // ثبت در AppUserRole
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
                else
                {
                    // اگر نقش "استاد" وجود نداشت، لاگ بزن
                    Console.WriteLine("⚠️ نقش 'استاد' در سیستم یافت نشد");
                }

                // ============================================================
                // 🔥 اگر کاربر نقش دیگری هم خواسته بود (اختیاری)
                // ============================================================
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
        // 4️⃣ آپلود گروهی اساتید از Excel (23 ستون)
        // ============================================================
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "فایل انتخاب نشده است" });

                if (!file.FileName.EndsWith(".xlsx"))
                    return BadRequest(new { success = false, message = "فرمت فایل باید xlsx باشد" });

                // ============================================================
                // 🔥 بررسی وجود نقش "استاد" در دیتابیس (قبل از هر گونه پردازش)
                // ============================================================
                var ostadRole = await _roleManager.FindByNameAsync("استاد");
                if (ostadRole == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "نقش 'استاد' در سیستم تعریف نشده است. لطفاً ابتدا نقش 'استاد' را ایجاد کنید."
                    });
                }

                // ============================================================
                // 🔥 خواندن کل فایل برای ذخیره داده‌های خطا
                // ============================================================
                var allRowsData = new List<List<string>>();
                var errors = new List<string>();
                var errorDetails = new List<ProcessedItem>();
                var rowCount = 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rowCountTotal = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                if (rowCountTotal < 2)
                    return BadRequest(new { success = false, message = "فایل خالی است" });

                // ============================================================
                // 🔥 ذخیره هدرها
                // ============================================================
                var headers = new List<string>();
                for (int col = 1; col <= 23; col++)
                {
                    headers.Add(worksheet.Cell(1, col).GetString()?.Trim() ?? $"ستون{col}");
                }

                // ============================================================
                // 🔥 خواندن تمام داده‌ها
                // ============================================================
                for (int row = 2; row <= rowCountTotal; row++)
                {
                    var rowData = new List<string>();
                    for (int col = 1; col <= 23; col++)
                    {
                        rowData.Add(worksheet.Cell(row, col).GetString()?.Trim() ?? "");
                    }
                    allRowsData.Add(rowData);
                }

                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);
                if (!accessibleMarkazIds.Any())
                    return BadRequest(new { success = false, message = "شما دسترسی به هیچ مرکزی برای افزودن استاد ندارید" });

                var allMarkazes = await _context.Markazes
                    .Where(m => m.Vazeeyat == true && m.CodeMarkaz != null)
                    .ToDictionaryAsync(m => m.CodeMarkaz!, m => m.Id);

                var allGroohes = await _context.GrooheAmoozeshis
                    .Where(g => g.CodeDaneshkade != null && g.CodeGrooheAmoozeshi != null)
                    .ToDictionaryAsync(
                        g => g.CodeDaneshkade! + "_" + g.CodeGrooheAmoozeshi!,
                        g => g.Id
                    );
               
                // ============================================================
                // 🔥 پردازش هر ردیف
                // ============================================================
                var errorRows = new List<List<string>>();

                for (int i = 0; i < allRowsData.Count; i++)
                {
                    var rowData = allRowsData[i];
                    var rowNumber = i + 2;
                    var processedItem = new ProcessedItem
                    {
                        RowNumber = rowNumber,
                        RowData = rowData
                    };

                    try
                    {
                        // ============================================================
                        // 1️⃣ خواندن داده‌ها از ردیف
                        // ============================================================
                        var codeMarkazKhedmati = rowData[0];
                        var codeMarkazAsli = rowData[1];
                        var codeOstadi = rowData[2];
                        var naamKhanevadegi = rowData[3];
                        var naam = rowData[4];
                        var jens = rowData[5];
                        var naamPedar = rowData[6];
                        var tarikhTavalod = rowData[7];
                        var shomareShenasname = rowData[8];
                        var shomareMelli = rowData[9];
                        var email = rowData[10];
                        var mobile1 = rowData[11];
                        var mobile2 = rowData[12];
                        var martabeElmi = rowData[13];
                        var noeHamkariText = rowData[14];
                        var noeBimeh = rowData[15];
                        var shomareBimeh = rowData[16];
                        var codeDaneshkadeh = rowData[17];
                        var codeGroohAmoozeshi = rowData[18];
                        var reshteh = rowData[19];
                        var grayesh = rowData[20];
                        var maghtaText = rowData[21];
                        var mahalAkhz = rowData[22];

                        processedItem.CodeOstadi = codeOstadi;
                        processedItem.ShomareMelli = shomareMelli;

                        // ============================================================
                        // 2️⃣ اعتبارسنجی
                        // ============================================================
                        if (string.IsNullOrEmpty(codeOstadi) || string.IsNullOrEmpty(shomareMelli))
                        {
                            errors.Add($"ردیف {rowNumber}: کد استادی و کد ملی الزامی است");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = "کد استادی و کد ملی الزامی است" });
                            continue;
                        }

                        if (string.IsNullOrEmpty(codeMarkazKhedmati))
                        {
                            errors.Add($"ردیف {rowNumber}: کد مرکز محل خدمت الزامی است");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = "کد مرکز محل خدمت الزامی است" });
                            continue;
                        }

                        // ============================================================
                        // 3️⃣ پیدا کردن MarkazId
                        // ============================================================
                        if (!allMarkazes.TryGetValue(codeMarkazKhedmati, out int markazKhedmatiId))
                        {
                            errors.Add($"ردیف {rowNumber}: کد مرکز '{codeMarkazKhedmati}' یافت نشد");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = $"کد مرکز '{codeMarkazKhedmati}' یافت نشد" });
                            continue;
                        }

                        if (!accessibleMarkazIds.Contains(markazKhedmatiId))
                        {
                            errors.Add($"ردیف {rowNumber}: شما دسترسی به مرکز '{codeMarkazKhedmati}' را ندارید");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = $"دسترسی به مرکز '{codeMarkazKhedmati}' ندارید" });
                            continue;
                        }

                        // ============================================================
                        // 4️⃣ بررسی تکراری
                        // ============================================================
                        var exists = await _context.Ostads.AnyAsync(o => o.CodeOstadi == codeOstadi);
                        if (exists)
                        {
                            errors.Add($"ردیف {rowNumber}: کد استادی {codeOstadi} قبلاً ثبت شده است");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = "کد استادی قبلاً ثبت شده است" });
                            continue;
                        }

                        var userExists = await _userManager.FindByNameAsync(codeOstadi);
                        if (userExists != null)
                        {
                            errors.Add($"ردیف {rowNumber}: کد استادی {codeOstadi} قبلاً به عنوان نام کاربری ثبت شده است");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = "کد استادی قبلاً به عنوان نام کاربری ثبت شده است" });
                            continue;
                        }

                        // ============================================================
                        // 5️⃣ پیدا کردن MarkazAsliId
                        // ============================================================
                        int? markazAsliId = null;
                        if (!string.IsNullOrEmpty(codeMarkazAsli))
                        {
                            if (!allMarkazes.TryGetValue(codeMarkazAsli, out int asliId))
                            {
                                errors.Add($"ردیف {rowNumber}: کد مرکز اصلی '{codeMarkazAsli}' یافت نشد");
                                errorRows.Add(rowData);
                                errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = $"کد مرکز اصلی '{codeMarkazAsli}' یافت نشد" });
                                continue;
                            }
                            markazAsliId = asliId;
                        }

                        // ============================================================
                        // 6️⃣ پیدا کردن GrooheAmoozeshiId
                        // ============================================================
                        int? grooheAmoozeshiId = null;
                        if (!string.IsNullOrEmpty(codeDaneshkadeh) && !string.IsNullOrEmpty(codeGroohAmoozeshi))
                        {
                            var key = codeDaneshkadeh + "_" + codeGroohAmoozeshi;
                            if (!allGroohes.TryGetValue(key, out int gId))
                            {
                                errors.Add($"ردیف {rowNumber}: ترکیب کد دانشکده '{codeDaneshkadeh}' و کد گروه '{codeGroohAmoozeshi}' یافت نشد");
                                errorRows.Add(rowData);
                                errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = $"ترکیب کد دانشکده '{codeDaneshkadeh}' و کد گروه '{codeGroohAmoozeshi}' یافت نشد" });
                                continue;
                            }
                            grooheAmoozeshiId = gId;
                        }
                        else if (!string.IsNullOrEmpty(codeDaneshkadeh) || !string.IsNullOrEmpty(codeGroohAmoozeshi))
                        {
                            errors.Add($"ردیف {rowNumber}: برای یافتن گروه آموزشی، هر دو کد دانشکده و کد گروه باید وارد شوند");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem { RowNumber = rowNumber, CodeOstadi = codeOstadi, ShomareMelli = shomareMelli, Status = "خطا", Message = "هر دو کد دانشکده و کد گروه باید وارد شوند" });
                            continue;
                        }

                        // ============================================================
                        // 7️⃣ تبدیل نوع همکاری
                        // ============================================================
                        int? noeHamkariValue = null;
                        if (!string.IsNullOrEmpty(noeHamkariText))
                        {
                            if (int.TryParse(noeHamkariText, out int nHamkari))
                                noeHamkariValue = nHamkari;
                            else
                            {
                                noeHamkariValue = noeHamkariText switch
                                {
                                    "هیات علمی پیام نور" => 1,
                                    "هیات علمی غیر پیام نور" => 2,
                                    "مدرس مدعو" => 3,
                                    "هیات علمی پیام نور (سایر استان ها)" => 4,
                                    _ => 3
                                };
                            }
                        }

                        // ============================================================
                        // 8️⃣ تبدیل مقطع
                        // ============================================================
                        int? maghtaValue = null;
                        if (!string.IsNullOrEmpty(maghtaText))
                        {
                            maghtaValue = maghtaText switch
                            {
                                "کارشناسی" => 5,
                                "کارشناسی ارشد" => 10,
                                "دکتری" => 15,
                                _ => int.TryParse(maghtaText, out int m) ? m : null
                            };
                        }

                        // ============================================================
                        // 9️⃣ ساخت اشیاء برای ذخیره با تراکنش
                        // ============================================================
                        var ostad = new Ostad
                        {
                            CodeOstadi = codeOstadi,
                            NaamKhanevadegi = naamKhanevadegi,
                            Naam = naam,
                            Jens = jens,
                            NaamPedar = naamPedar,
                            TarikhTavalod = tarikhTavalod ?? "",
                            ShomareShenasname = shomareShenasname,
                            ShomareMelli = shomareMelli,
                            Email = email,
                            Mobile = mobile1,
                            Mobile2 = mobile2,
                            MarkazId = markazKhedmatiId,
                            MarkazAsliId = markazAsliId,
                            MartabeElmi = martabeElmi,
                            NoeHamkari = (NoeHamkariEnum?)noeHamkariValue,
                            NoeBimeh = noeBimeh,
                            ShomarehBimeh = shomareBimeh
                        };

                        var madrak = new OstadMadrak
                        {
                            Reshteh = reshteh,
                            Grayesh = grayesh,
                            Maghta = maghtaValue,
                            MahalAkhz = mahalAkhz,
                            GrooheAmoozeshiId = grooheAmoozeshiId,
                            PishFarz = true
                        };

                        var user = new AppUser
                        {
                            UserName = codeOstadi,
                            Email = email,
                            Vazeeyat = true,
                            VazeeyatMovaghat = true
                        };

                        // ============================================================
                        // 🔟 ذخیره با تراکنش
                        // ============================================================
                        using var transaction = await _context.Database.BeginTransactionAsync();

                        try
                        {
                            // 1️⃣ ذخیره استاد
                            await _context.Ostads.AddAsync(ostad);
                            await _context.SaveChangesAsync();

                            // 2️⃣ ذخیره مدرک تحصیلی
                            madrak.OstadId = ostad.Id;
                            await _context.OstadMadraks.AddAsync(madrak);
                            await _context.SaveChangesAsync();

                            // 3️⃣ ایجاد کاربر
                            user.OstadId = ostad.Id;
                            var password = shomareMelli + "aA";
                            var createUserResult = await _userManager.CreateAsync(user, password);

                            if (!createUserResult.Succeeded)
                            {
                                throw new Exception($"خطا در ایجاد کاربر: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                            }

                            // ============================================================
                            // 🔥 4️⃣ اضافه کردن نقش "استاد" به صورت پیش‌فرض
                            // ============================================================

                            // بررسی اینکه کاربر قبلاً این نقش را ندارد
                            var existingRole = await _context.Set<AppUserRole>()
                                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == ostadRole.Id);

                            if (existingRole == null)
                            {
                                // ✅ اضافه کردن نقش به کاربر (با نام نقش)
                                await _userManager.AddToRoleAsync(user, "استاد");

                                // ثبت در AppUserRole
                                var appUserRole = new AppUserRole
                                {
                                    UserId = user.Id,
                                    RoleId = ostadRole.Id,
                                    MarkazId = markazKhedmatiId,
                                    RolePishFarz = true
                                };

                                _context.Set<AppUserRole>().Add(appUserRole);
                                await _context.SaveChangesAsync();
                            }

                            await transaction.CommitAsync();
                            rowCount++;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            errors.Add($"ردیف {rowNumber}: خطا در ثبت {codeOstadi} - {ex.Message}");
                            errorRows.Add(rowData);
                            errorDetails.Add(new ProcessedItem
                            {
                                RowNumber = rowNumber,
                                CodeOstadi = codeOstadi,
                                ShomareMelli = shomareMelli,
                                Status = "خطا",
                                Message = ex.Message
                            });
                        }

                    }
                    catch (Exception ex)
                    {
                        errors.Add($"ردیف {rowNumber}: خطا در پردازش - {ex.Message}");
                        errorRows.Add(rowData);
                        errorDetails.Add(new ProcessedItem
                        {
                            RowNumber = rowNumber,
                            CodeOstadi = rowData[2],
                            ShomareMelli = rowData[9],
                            Status = "خطا",
                            Message = ex.Message
                        });
                    }
                }

                // ============================================================
                // 🔥 تولید فایل اکسل خطاها (اگر خطایی وجود داشت)
                // ============================================================
                byte[]? errorFileBytes = null;
                if (errorRows.Any())
                {
                    using var errorWorkbook = new XLWorkbook();
                    var errorWorksheet = errorWorkbook.Worksheets.Add("خطاها");

                    // ============================================================
                    // 🔥 نوشتن هدرها (با یک ستون اضافی برای توضیح خطا)
                    // ============================================================
                    for (int col = 1; col <= headers.Count; col++)
                    {
                        errorWorksheet.Cell(1, col).Value = headers[col - 1];
                    }
                    errorWorksheet.Cell(1, headers.Count + 1).Value = "توضیح خطا";

                    // ============================================================
                    // 🔥 نوشتن داده‌های خطا
                    // ============================================================
                    for (int row = 0; row < errorRows.Count; row++)
                    {
                        var rowData = errorRows[row];
                        for (int col = 0; col < rowData.Count; col++)
                        {
                            errorWorksheet.Cell(row + 2, col + 1).Value = rowData[col];
                        }
                        // ستون آخر: توضیح خطا
                        var errorMsg = errorDetails.FirstOrDefault(e => e.RowNumber == row + 2)?.Message ?? "خطای ناشناخته";
                        errorWorksheet.Cell(row + 2, headers.Count + 1).Value = errorMsg;
                    }

                    // ============================================================
                    // 🔥 تنظیم عرض ستون‌ها
                    // ============================================================
                    errorWorksheet.Columns().AdjustToContents();

                    using var ms = new MemoryStream();
                    errorWorkbook.SaveAs(ms);
                    errorFileBytes = ms.ToArray();
                }

                // ============================================================
                // 🔥 پاسخ نهایی
                // ============================================================
                var result = new BulkUploadResult
                {
                    Success = true,
                    Message = $"تعداد {rowCount} استاد با موفقیت ثبت شد",
                    TotalRows = rowCountTotal - 1,
                    SuccessCount = rowCount,
                    ErrorCount = errorRows.Count,
                    Errors = errors,
                    Details = errorDetails,
                    ErrorFileBytes = errorFileBytes,
                    ErrorFileName = errorRows.Any() ? "خطاهای_بارگذاری_اساتید.xlsx" : null
                };

                // ============================================================
                // 🔥 اگر فایل خطا وجود دارد، به صورت فایل برگردان
                // ============================================================
                if (errorFileBytes != null)
                {
                    return File(errorFileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "خطاهای_بارگذاری_اساتید.xlsx");
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    totalRows = result.TotalRows,
                    successCount = result.SuccessCount,
                    errorCount = result.ErrorCount,
                    errors = result.Errors.Any() ? result.Errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در آپلود فایل", error = ex.Message });
            }
        }

        public class BulkUploadResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public int TotalRows { get; set; }
            public int SuccessCount { get; set; }
            public int ErrorCount { get; set; }
            public List<string> Errors { get; set; } = new();
            public List<ProcessedItem> Details { get; set; } = new();
            public byte[]? ErrorFileBytes { get; set; }  // ← فایل اکسل خطاها
            public string? ErrorFileName { get; set; }
        }

        public class ProcessedItem
        {
            public int RowNumber { get; set; }
            public string? CodeOstadi { get; set; }
            public string? ShomareMelli { get; set; }
            public string? Status { get; set; }
            public string? Message { get; set; }
            public List<string>? RowData { get; set; }  // ← داده‌های کامل ردیف برای فایل خطا
        }

        private async Task SaveOstadBatch(List<Ostad> ostads, List<AppUser> users, List<OstadMadrak> madraks)
        {
            // ============================================================
            // 1️⃣ ذخیره اساتید
            // ============================================================
            await _context.Ostads.AddRangeAsync(ostads);
            await _context.SaveChangesAsync();

            // ============================================================
            // 2️⃣ تخصیص OstadId به مدارک و ذخیره
            // ============================================================
            for (int i = 0; i < ostads.Count && i < madraks.Count; i++)
            {
                madraks[i].OstadId = ostads[i].Id;
            }

            if (madraks.Any())
            {
                await _context.OstadMadraks.AddRangeAsync(madraks);
                await _context.SaveChangesAsync();
            }

            // ============================================================
            // 3️⃣ ذخیره کاربران و اضافه کردن نقش "استاد"
            // ============================================================
            var ostadRole = await _roleManager.FindByNameAsync("استاد");

            foreach (var user in users)
            {
                var ostad = ostads.FirstOrDefault(o => o.Id == user.OstadId);
                if (ostad != null)
                {
                    var password = ostad.ShomareMelli + "aA";
                    await _userManager.CreateAsync(user, password);

                    // ============================================================
                    // 🔥 اضافه کردن نقش "استاد"
                    // ============================================================
                    if (ostadRole != null)
                    {
                        // بررسی اینکه کاربر قبلاً این نقش را ندارد
                        var existingRole = await _context.Set<AppUserRole>()
                            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == ostadRole.Id);

                        if (existingRole == null)
                        {
                            await _userManager.AddToRoleAsync(user, ostadRole.Name);

                            var appUserRole = new AppUserRole
                            {
                                UserId = user.Id,
                                RoleId = ostadRole.Id,
                                MarkazId = ostad.MarkazId,
                                RolePishFarz = true
                            };
                            await _context.Set<AppUserRole>().AddAsync(appUserRole);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // 5️⃣ ویرایش استاد
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OstadUpdateDto dto)
        {
            try
            {
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
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                var ostad = await _context.Ostads.FindAsync(id);
                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.OstadId == id);
                if (user != null)
                    await _userManager.DeleteAsync(user);

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
        // 7️⃣ تغییر وضعیت گروهی اساتید (فقط ادمین سامانه - CodeRole=1)
        // ============================================================
        [HttpPatch("toggle")]
        public async Task<IActionResult> Toggle([FromBody] List<ToggleOstadStatusItemDto> items)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
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

                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

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