using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Karmand;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class KarmandController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public KarmandController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // 1️⃣ دریافت لیست کارمندان با صفحه‌بندی و فیلتر
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null,
            [FromQuery] bool? vazeeat = null)
        {
            try
            {
                // ============================================================
                // 1️⃣ ساخت کوئری پایه با Join به AppUser برای وضعیت
                // ============================================================
                var query = from k in _context.Karmands
                            join u in _context.Users on k.Id equals u.KarmandId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            select new { Karmand = k, User = u };

                // ============================================================
                // 2️⃣ فیلتر بر اساس جستجو (نام، نام خانوادگی، کد ملی)
                // ============================================================
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.Karmand.Naam != null && x.Karmand.Naam.Contains(search)) ||
                        (x.Karmand.NaameKhanevadeghi != null && x.Karmand.NaameKhanevadeghi.Contains(search)) ||
                        (x.Karmand.CodeMelli != null && x.Karmand.CodeMelli.Contains(search)));
                }

                // ============================================================
                // 3️⃣ فیلتر بر اساس استان و مرکز
                // ============================================================
                if (ostanId.HasValue && !markazId.HasValue)
                {
                    // فقط استان - همه مراکز آن استان
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Karmand.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Karmand.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue)
                {
                    // استان و مرکز خاص
                    query = query.Where(x => x.Karmand.MarkazId == markazId.Value);
                }

                // ============================================================
                // 4️⃣ فیلتر بر اساس وضعیت (Vazeeat از AppUser)
                // ============================================================
                if (vazeeat.HasValue)
                {
                    query = query.Where(x => x.User != null && x.User.Vazeeyat == vazeeat.Value);
                }
                else
                {
                    // پیش‌فرض: فقط کاربران فعال
                    query = query.Where(x => x.User == null || x.User.Vazeeyat == true);
                }

                // ============================================================
                // 5️⃣ محاسبه تعداد کل رکوردها
                // ============================================================
                var totalCount = await query.CountAsync();

                // ============================================================
                // 6️⃣ اعمال صفحه‌بندی
                // ============================================================
                var karmands = await query
                    .OrderBy(x => x.Karmand.NaameKhanevadeghi)
                    .ThenBy(x => x.Karmand.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new KarmandListDto
                    {
                        Id = x.Karmand.Id,
                        CodeMelli = x.Karmand.CodeMelli ?? "",
                        Naam = x.Karmand.Naam ?? "",
                        NaameKhanevadeghi = x.Karmand.NaameKhanevadeghi ?? "",
                        MarkazId = x.Karmand.MarkazId ?? 0,
                        MarkazName = _context.Markazes
                            .Where(m => m.Id == x.Karmand.MarkazId)
                            .Select(m => m.NaamMarkaz ?? "")
                            .FirstOrDefault() ?? "",
                        Mobile = x.Karmand.Mobile ?? "",
                        Email = x.Karmand.Email ?? "",
                        Vazeeat = x.User != null ? x.User.Vazeeyat ?? true : true
                    })
                    .ToListAsync();

                // ============================================================
                // 7️⃣ برگرداندن پاسخ با اطلاعات صفحه‌بندی
                // ============================================================
                return Ok(new
                {
                    success = true,
                    message = "لیست کارمندان دریافت شد",
                    data = karmands,
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
                    message = "خطا در دریافت کارمندان",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک کارمند
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var karmand = await _context.Karmands
                    .Include(k => k.Markaz)
                    .Include(k => k.MarkazAsli)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                var dto = new KarmandDetailDto
                {
                    Id = karmand.Id,
                    CodeMelli = karmand.CodeMelli ?? "",
                    Naam = karmand.Naam ?? "",
                    NaameKhanevadeghi = karmand.NaameKhanevadeghi ?? "",
                    MarkazId = karmand.MarkazId ?? 0,
                    MarkazName = karmand.Markaz?.NaamMarkaz ?? "",
                    MarkazAsliId = karmand.MarkazAsliId ?? 0,
                    MarkazAsliName = karmand.MarkazAsli?.NaamMarkaz ?? "",
                    Mobile = karmand.Mobile ?? "",
                    Mobile2 = karmand.Mobile2 ?? "",
                    TelefonMostaghim = karmand.TelefonMostaghim ?? "",
                    TelefonGhayreMostaghim = karmand.TelefonGhayreMostaghim ?? "",
                    TelefonDakheli = karmand.TelefonDakheli ?? "",
                    Email = karmand.Email ?? "",
                    Emza = karmand.Emza ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات کارمند دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد کارمند جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] KarmandCreateDto dto)
        {
            try
            {
                // بررسی کد ملی تکراری
                var exists = await _context.Karmands
                    .AnyAsync(k => k.CodeMelli == dto.CodeMelli);

                if (exists)
                    return BadRequest(new { success = false, message = "کد ملی قبلاً ثبت شده است" });

                // بررسی تکراری بودن نام کاربری
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "نام کاربری قبلاً ثبت شده است" });

                // ============================================================
                // 1️⃣ ایجاد کارمند
                // ============================================================
                var karmand = new Karmand
                {
                    CodeMelli = dto.CodeMelli,
                    Naam = dto.Naam,
                    NaameKhanevadeghi = dto.NaameKhanevadeghi,
                    MarkazId = dto.MarkazId,
                    MarkazAsliId = dto.MarkazAsliId,
                    Mobile = dto.Mobile,
                    Mobile2 = dto.Mobile2,
                    TelefonMostaghim = dto.TelefonMostaghim,
                    TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim,
                    TelefonDakheli = dto.TelefonDakheli,
                    Email = dto.Email,
                    Emza = dto.Emza
                };

                await _context.Karmands.AddAsync(karmand);
                await _context.SaveChangesAsync();

                // ============================================================
                // 2️⃣ ایجاد کاربر متناظر
                // ============================================================
                var user = new AppUser
                {
                    UserName = dto.UserName,  // نام کاربری دستی
                    Email = dto.Email,
                    KarmandId = karmand.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = true
                };

                var password = dto.CodeMelli; // رمز = کد ملی
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _context.Karmands.Remove(karmand);
                    await _context.SaveChangesAsync();

                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد کاربر",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // اضافه کردن نقش
                if (!string.IsNullOrEmpty(dto.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, dto.RoleName);
                }

                return Ok(new
                {
                    success = true,
                    message = "کارمند و کاربر با موفقیت ایجاد شد",
                    data = new { karmandId = karmand.Id, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش کارمند
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] KarmandUpdateDto dto)
        {
            try
            {
                var karmand = await _context.Karmands.FindAsync(id);
                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                karmand.Naam = dto.Naam ?? karmand.Naam;
                karmand.NaameKhanevadeghi = dto.NaameKhanevadeghi ?? karmand.NaameKhanevadeghi;
                karmand.MarkazId = dto.MarkazId ?? karmand.MarkazId;
                karmand.MarkazAsliId = dto.MarkazAsliId ?? karmand.MarkazAsliId;
                karmand.Mobile = dto.Mobile ?? karmand.Mobile;
                karmand.Mobile2 = dto.Mobile2 ?? karmand.Mobile2;
                karmand.TelefonMostaghim = dto.TelefonMostaghim ?? karmand.TelefonMostaghim;
                karmand.TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim ?? karmand.TelefonGhayreMostaghim;
                karmand.TelefonDakheli = dto.TelefonDakheli ?? karmand.TelefonDakheli;
                karmand.Email = dto.Email ?? karmand.Email;
                karmand.Emza = dto.Emza ?? karmand.Emza;

                await _context.SaveChangesAsync();

                // به‌روزرسانی ایمیل کاربر
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.KarmandId == id);

                if (user != null && !string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { success = true, message = "کارمند ویرایش شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 5️⃣ حذف کارمند
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var karmand = await _context.Karmands.FindAsync(id);
                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.KarmandId == id);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Karmands.Remove(karmand);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "کارمند و کاربر مربوطه حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف کارمند", error = ex.Message });
            }
        }
    }
}