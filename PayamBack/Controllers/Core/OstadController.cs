using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PayamBack.Data;
using PayamBack.DTOs.Core.Ostad;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class OstadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OstadController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            [FromQuery] bool? vazeeat = null)  // ← پارامتر جدید
        {
            try
            {
                // ============================================================
                // 1️⃣ ساخت کوئری پایه
                // ============================================================
                var query = _context.Ostads
                    .Include(o => o.Markaz)
                    .AsQueryable();

                // ============================================================
                // 2️⃣ فیلتر بر اساس جستجو (نام، نام خانوادگی، کد استادی)
                // ============================================================
                if (!string.IsNullOrEmpty(search))
                {
                    // اگر عددی است، بر اساس کد استادی جستجو کن
                    if (int.TryParse(search, out _))
                    {
                        query = query.Where(o => o.CodeOstadi != null && o.CodeOstadi.Contains(search));
                    }
                    else
                    {
                        // در غیر این صورت بر اساس نام یا نام خانوادگی
                        query = query.Where(o =>
                            (o.Naam != null && o.Naam.Contains(search)) ||
                            (o.NaamKhanevadegi != null && o.NaamKhanevadegi.Contains(search)));
                    }
                }

                // ============================================================
                // 3️⃣ فیلتر بر اساس رشته تحصیلی
                // ============================================================
                if (!string.IsNullOrEmpty(reshteh))
                {
                    query = query.Where(o =>
                        _context.OstadMadraks
                            .Any(m => m.OstadId == o.Id && m.Reshteh != null && m.Reshteh.Contains(reshteh)));
                }

                // ============================================================
                // 4️⃣ فیلتر بر اساس استان و مرکز
                // ============================================================
                if (ostanId.HasValue && !markazId.HasValue)
                {
                    // فقط استان - همه مراکز آن استان
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(o => o.MarkazId.HasValue && markazIdsInOstan.Contains(o.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue)
                {
                    // استان و مرکز خاص
                    query = query.Where(o => o.MarkazId == markazId.Value);
                }

                // ============================================================
                // 5️⃣ فیلتر بر اساس نوع همکاری
                // ============================================================
                if (noeHamkari.HasValue)
                {
                    query = query.Where(o => o.NoeHamkari == (NoeHamkariEnum)noeHamkari.Value);
                }

                // ============================================================
                // 6️⃣ 🔥 فیلتر بر اساس وضعیت استاد (فعال/غیرفعال)
                // ============================================================
                if (vazeeat.HasValue)
                {
                    query = query.Where(o => o.Vazeeat == vazeeat.Value);
                }
                // اگر vazeeat null باشد، همه اساتید (هم فعال و هم غیرفعال) نمایش داده می‌شوند

                // ============================================================
                // 7️⃣ محاسبه تعداد کل رکوردها
                // ============================================================
                var totalCount = await query.CountAsync();

                // ============================================================
                // 🔥 صفحه‌بندی با نام مرکز و رشته تحصیلی پیش‌فرض
                // ============================================================
                var ostads = await query
                    .OrderBy(o => o.NaamKhanevadegi)
                    .ThenBy(o => o.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new OstadListDto
                    {
                        Id = o.Id,
                        CodeOstadi = o.CodeOstadi ?? "",
                        Naam = o.Naam ?? "",
                        NaamKhanevadegi = o.NaamKhanevadegi ?? "",
                        MarkazId = o.MarkazId ?? 0,
                        MarkazName = o.Markaz != null ? o.Markaz.NaamMarkaz ?? "" : "",
                        NoeHamkari = (int)(o.NoeHamkari ?? 0),
                        MartabeElmi = o.MartabeElmi ?? "",
                        Vazeeat = o.Vazeeat ?? true,

                        // ============================================================
                        // 🔥 دریافت رشته تحصیلی پیش‌فرض استاد از OstadMadrak
                        // ============================================================
                        Reshteh = _context.OstadMadraks
                            .Where(m => m.OstadId == o.Id && m.PishFarz == true)
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
                    Vazeeat = ostad.Vazeeat ?? true,
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
                    Vazeeat = true,
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
                    VazeeyatMovaghat = false
                };

                var password = dto.ShomareMelli;
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
                            Vazeeat = true
                        };

                        ostads.Add(ostad);

                        var user = new AppUser
                        {
                            UserName = codeOstadi,
                            Email = email,
                            OstadId = ostad.Id,
                            Vazeeyat = true,
                            VazeeyatMovaghat = false
                        };
                        users.Add(user);

                        rowCount++;

                        // ذخیره هر 200 رکورد
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

                // ذخیره باقیمانده
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
                var password = ostads.First(o => o.Id == user.OstadId)?.ShomareMelli ?? "123456";
                await _userManager.CreateAsync(user, password);
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
                var ostad = await _context.Ostads.FindAsync(id);
                if (ostad == null)
                    return NotFound(new { success = false, message = "استاد یافت نشد" });

                ostad.Naam = dto.Naam ?? ostad.Naam;
                ostad.NaamKhanevadegi = dto.NaamKhanevadegi ?? ostad.NaamKhanevadegi;
                ostad.MarkazId = dto.MarkazId ?? ostad.MarkazId;
                ostad.MarkazAsliId = dto.MarkazAsliId ?? ostad.MarkazAsliId;
                ostad.Jens = dto.Jens ?? ostad.Jens;
                ostad.Email = dto.Email ?? ostad.Email;
                ostad.Mobile = dto.Mobile ?? ostad.Mobile;
                ostad.Mobile2 = dto.Mobile2 ?? ostad.Mobile2;
                ostad.Vazeeat = dto.Vazeeat ?? ostad.Vazeeat;
                ostad.NoeHamkari = dto.NoeHamkari ?? ostad.NoeHamkari;
                ostad.NoeBimeh = dto.NoeBimeh ?? ostad.NoeBimeh;
                ostad.ShomarehBimeh = dto.ShomarehBimeh ?? ostad.ShomarehBimeh;

                await _context.SaveChangesAsync();

                // به‌روزرسانی ایمیل کاربر
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
        // 6️⃣ حذف استاد
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
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
    }
}