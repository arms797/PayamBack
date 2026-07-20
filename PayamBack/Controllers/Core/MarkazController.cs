using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Markaz;
using PayamBack.Models.Core;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  // ← کل کنترلر بدون احراز هویت (برای نمایش عمومی)
    public class MarkazController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MarkazController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه مراکز (فعال)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var markazes = await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .OrderBy(m => m.NaamMarkaz)
                    .Select(m => new MarkazListDto
                    {
                        Id = m.Id,
                        CodeMarkaz = m.CodeMarkaz ?? "",
                        NaamMarkaz = m.NaamMarkaz ?? "",
                        CodeOstan = m.CodeOstan ?? "",
                        NaamOstan = m.NaamOstan ?? "",
                        Vazeeyat = m.Vazeeyat ?? false,
                        Level = m.Level ?? 4  // ← اضافه شد
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مراکز با موفقیت دریافت شد",
                    data = markazes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست مراکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت اطلاعات یک مرکز
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var markaz = await _context.Markazes
                    .Where(m => m.Id == id)
                    .Select(m => new MarkazDetailDto
                    {
                        Id = m.Id,
                        CodeMarkaz = m.CodeMarkaz ?? "",
                        NaamMarkaz = m.NaamMarkaz ?? "",
                        CodeOstan = m.CodeOstan ?? "",
                        NaamOstan = m.NaamOstan ?? "",
                        VahedMarkaz = m.VahedMarkaz ?? "",
                        Nahiyeh = m.Nahiyeh ?? "",
                        MahalMarkaz = m.MahalMarkaz ?? "",
                        Adres = m.Adres ?? "",
                        CodePosti = m.CodePosti ?? "",
                        WebSite = m.WebSite ?? "",
                        Telefon = m.Telefon ?? "",
                        Vazeeyat = m.Vazeeyat ?? false,
                        Level = m.Level ?? 4  // ← اضافه شد
                    })
                    .FirstOrDefaultAsync();

                if (markaz == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مرکز مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات مرکز با موفقیت دریافت شد",
                    data = markaz
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات مرکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد مرکز جدید (نیاز به مجوز)
        // ============================================================
        [HttpPost("create")]
        [Authorize]  // ← برای ایجاد نیاز به احراز هویت دارد
        public async Task<IActionResult> Create([FromBody] MarkazCreateDto dto)
        {
            try
            {
                // بررسی تکراری نبودن کد مرکز
                var exists = await _context.Markazes
                    .AnyAsync(m => m.CodeMarkaz == dto.CodeMarkaz);

                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "کد مرکز قبلاً ثبت شده است"
                    });
                }

                var markaz = new Markaz
                {
                    CodeMarkaz = dto.CodeMarkaz,
                    NaamMarkaz = dto.NaamMarkaz,
                    CodeOstan = dto.CodeOstan,
                    NaamOstan = dto.NaamOstan,
                    VahedMarkaz = dto.VahedMarkaz,
                    Nahiyeh = dto.Nahiyeh,
                    MahalMarkaz = dto.MahalMarkaz,
                    Adres = dto.Adres,
                    CodePosti = dto.CodePosti,
                    WebSite = dto.WebSite,
                    Telefon = dto.Telefon,
                    Vazeeyat = dto.Vazeeyat ?? true,
                    Dakheli = dto.Dakheli ?? true,
                    Level = dto.Level ?? 4  // ← اضافه شد
                };

                await _context.Markazes.AddAsync(markaz);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مرکز با موفقیت ایجاد شد",
                    data = new { id = markaz.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد مرکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش مرکز (نیاز به مجوز)
        // ============================================================
        [HttpPut("update/{id}")]
        [Authorize]  // ← برای ویرایش نیاز به احراز هویت دارد
        public async Task<IActionResult> Update(int id, [FromBody] MarkazUpdateDto dto)
        {
            try
            {
                var markaz = await _context.Markazes.FindAsync(id);
                if (markaz == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مرکز مورد نظر یافت نشد"
                    });
                }

                // به‌روزرسانی فیلدها
                markaz.NaamMarkaz = dto.NaamMarkaz ?? markaz.NaamMarkaz;
                markaz.CodeOstan = dto.CodeOstan ?? markaz.CodeOstan;
                markaz.NaamOstan = dto.NaamOstan ?? markaz.NaamOstan;
                markaz.VahedMarkaz = dto.VahedMarkaz ?? markaz.VahedMarkaz;
                markaz.Nahiyeh = dto.Nahiyeh ?? markaz.Nahiyeh;
                markaz.MahalMarkaz = dto.MahalMarkaz ?? markaz.MahalMarkaz;
                markaz.Adres = dto.Adres ?? markaz.Adres;
                markaz.CodePosti = dto.CodePosti ?? markaz.CodePosti;
                markaz.WebSite = dto.WebSite ?? markaz.WebSite;
                markaz.Telefon = dto.Telefon ?? markaz.Telefon;
                markaz.Vazeeyat = dto.Vazeeyat ?? markaz.Vazeeyat;
                markaz.Dakheli = dto.Dakheli ?? markaz.Dakheli;
                markaz.Level = dto.Level ?? markaz.Level;  // ← اضافه شد

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مرکز با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش مرکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف مرکز (نیاز به مجوز)
        // ============================================================
        [HttpDelete("delete/{id}")]
        [Authorize]  // ← برای حذف نیاز به احراز هویت دارد
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var markaz = await _context.Markazes.FindAsync(id);
                if (markaz == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مرکز مورد نظر یافت نشد"
                    });
                }

                // بررسی اینکه مرکز به جای دیگری متصل نباشد
                var isUsed = await _context.Ostads.AnyAsync(o => o.MarkazId == id) ||
                             await _context.Karmands.AnyAsync(k => k.MarkazId == id) ||
                             await _context.Daneshjoos.AnyAsync(d => d.MarkazId == id);

                if (isUsed)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این مرکز به کاربران متصل است و قابل حذف نیست"
                    });
                }

                _context.Markazes.Remove(markaz);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مرکز با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف مرکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ دریافت مراکز بر اساس سطح (Level)
        // ============================================================
        [HttpGet("by-level/{level}")]
        [Authorize]
        public async Task<IActionResult> GetByLevel(int level)
        {
            try
            {
                var markazes = await _context.Markazes
                    .Where(m => m.Level == level && m.Vazeeyat == true)
                    .OrderBy(m => m.NaamMarkaz)
                    .Select(m => new MarkazListDto
                    {
                        Id = m.Id,
                        CodeMarkaz = m.CodeMarkaz ?? "",
                        NaamMarkaz = m.NaamMarkaz ?? "",
                        CodeOstan = m.CodeOstan ?? "",
                        NaamOstan = m.NaamOstan ?? "",
                        Vazeeyat = m.Vazeeyat ?? false,
                        Level = m.Level ?? 4
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مراکز با موفقیت دریافت شد",
                    data = markazes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت مراکز",
                    error = ex.Message
                });
            }
        }
    }
}