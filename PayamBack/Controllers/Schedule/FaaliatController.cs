using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Schedule.Faaliat;
using PayamBack.Models.Schedule;

namespace PayamBack.Controllers.Schedule
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FaaliatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FaaliatController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // متد کمکی برای نمایش نوع انجام
        // ============================================================
        private string GetNoeAnjamDisplay(int? noeAnjam)
        {
            return noeAnjam switch
            {
                1 => "حضوری",
                2 => "مجازی",
                3 => "ترکیبی",
                _ => ""
            };
        }

        // ============================================================
        // دریافت لیست کامل فعالیت‌ها (بدون فیلتر و صفحه‌بندی)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var items = await _context.Set<Faaliat>()
                    .OrderBy(f => f.Onvan)
                    .Select(f => new FaaliatListDto
                    {
                        Id = f.Id,
                        Onvan = f.Onvan ?? "",
                        NoeAnjam = f.NoeAnjam ?? 0,
                        //NoeAnjamDisplay = GetNoeAnjamDisplay(f.NoeAnjam),
                        MinSaatDarHafteh = f.MinSaatDarHafteh,
                        MaxSaatDarHafteh = f.MaxSaatDarHafteh,
                        IsMadove = f.IsMadove ?? false,
                        Color = f.Color ?? "",
                        Vazeeat = f.Vazeeat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست فعالیت‌ها دریافت شد",
                    data = items
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست فعالیت‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت لیست فعالیت‌های فعال (برای استفاده در کامبوباکس)
        // ============================================================
        [HttpGet("active-list")]
        public async Task<IActionResult> GetActiveList()
        {
            try
            {
                var items = await _context.Set<Faaliat>()
                    .Where(f => f.Vazeeat == true)
                    .OrderBy(f => f.Onvan)
                    .Select(f => new
                    {
                        f.Id,
                        f.Onvan,
                        f.NoeAnjam,
                        NoeAnjamDisplay = GetNoeAnjamDisplay(f.NoeAnjam),
                        f.Color,
                        f.MinSaatDarHafteh,
                        f.MaxSaatDarHafteh,
                        f.MinDayDarHafteh,
                        f.MaxDayDarHafteh
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست فعالیت‌های فعال دریافت شد",
                    data = items
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست فعالیت‌های فعال",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ دریافت یک فعالیت
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var faaliat = await _context.Set<Faaliat>()
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (faaliat == null)
                    return NotFound(new { success = false, message = "فعالیت یافت نشد" });

                var dto = new FaaliatDetailDto
                {
                    Id = faaliat.Id,
                    Onvan = faaliat.Onvan ?? "",
                    NoeAnjam = faaliat.NoeAnjam ?? 0,
                    NoeAnjamDisplay = GetNoeAnjamDisplay(faaliat.NoeAnjam),
                    MinSaatDarEdari = faaliat.MinSaatDarEdari,
                    MaxSaatDarEdari = faaliat.MaxSaatDarEdari,
                    MinSaatDarHafteh = faaliat.MinSaatDarHafteh,
                    MaxSaatDarHafteh = faaliat.MaxSaatDarHafteh,
                    MinDayDarHafteh = faaliat.MinDayDarHafteh,
                    MaxDayDarHafteh = faaliat.MaxDayDarHafteh,
                    IsMadove = faaliat.IsMadove ?? false,
                    Color = faaliat.Color ?? "",
                    Vazeeat = faaliat.Vazeeat ?? false
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات فعالیت دریافت شد",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات فعالیت",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ایجاد فعالیت جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FaaliatCreateDto dto)
        {
            try
            {
                // بررسی تکراری بودن عنوان
                var exists = await _context.Set<Faaliat>()
                    .AnyAsync(f => f.Onvan == dto.Onvan);

                if (exists)
                    return BadRequest(new { success = false, message = "فعالیتی با این عنوان قبلاً ثبت شده است" });

                var faaliat = new Faaliat
                {
                    Onvan = dto.Onvan,
                    NoeAnjam = dto.NoeAnjam,
                    MinSaatDarEdari = dto.MinSaatDarEdari,
                    MaxSaatDarEdari = dto.MaxSaatDarEdari,
                    MinSaatDarHafteh = dto.MinSaatDarHafteh,
                    MaxSaatDarHafteh = dto.MaxSaatDarHafteh,
                    MinDayDarHafteh = dto.MinDayDarHafteh,
                    MaxDayDarHafteh = dto.MaxDayDarHafteh,
                    IsMadove = dto.IsMadove ?? false,
                    Color = dto.Color,
                    Vazeeat = dto.Vazeeat ?? true
                };

                await _context.Set<Faaliat>().AddAsync(faaliat);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "فعالیت با موفقیت ایجاد شد",
                    data = new { id = faaliat.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد فعالیت",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ ویرایش فعالیت
        // ============================================================
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] FaaliatUpdateDto dto)
        {
            try
            {
                var faaliat = await _context.Set<Faaliat>()
                    .FirstOrDefaultAsync(f => f.Id == dto.Id);

                if (faaliat == null)
                    return NotFound(new { success = false, message = "فعالیت یافت نشد" });

                // بررسی تکراری بودن عنوان (در صورت تغییر)
                if (!string.IsNullOrEmpty(dto.Onvan) && dto.Onvan != faaliat.Onvan)
                {
                    var exists = await _context.Set<Faaliat>()
                        .AnyAsync(f => f.Onvan == dto.Onvan && f.Id != dto.Id);

                    if (exists)
                        return BadRequest(new { success = false, message = "فعالیتی با این عنوان قبلاً ثبت شده است" });
                }

                // به‌روزرسانی
                if (!string.IsNullOrEmpty(dto.Onvan)) faaliat.Onvan = dto.Onvan;
                if (dto.NoeAnjam.HasValue) faaliat.NoeAnjam = dto.NoeAnjam;
                if (dto.MinSaatDarEdari.HasValue) faaliat.MinSaatDarEdari = dto.MinSaatDarEdari;
                if (dto.MaxSaatDarEdari.HasValue) faaliat.MaxSaatDarEdari = dto.MaxSaatDarEdari;
                if (dto.MinSaatDarHafteh.HasValue) faaliat.MinSaatDarHafteh = dto.MinSaatDarHafteh;
                if (dto.MaxSaatDarHafteh.HasValue) faaliat.MaxSaatDarHafteh = dto.MaxSaatDarHafteh;
                if (dto.MinDayDarHafteh.HasValue) faaliat.MinDayDarHafteh = dto.MinDayDarHafteh;
                if (dto.MaxDayDarHafteh.HasValue) faaliat.MaxDayDarHafteh = dto.MaxDayDarHafteh;
                if (dto.IsMadove.HasValue) faaliat.IsMadove = dto.IsMadove;
                if (!string.IsNullOrEmpty(dto.Color)) faaliat.Color = dto.Color;
                if (dto.Vazeeat.HasValue) faaliat.Vazeeat = dto.Vazeeat;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "فعالیت با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش فعالیت",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ حذف فعالیت
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var faaliat = await _context.Set<Faaliat>()
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (faaliat == null)
                    return NotFound(new { success = false, message = "فعالیت یافت نشد" });

                // بررسی استفاده شدن در Hamjavar1
                var isUsed = await _context.Set<Hamjavar1>()
                    .AnyAsync(h => h.FaaliatId == id);

                if (isUsed)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این فعالیت در درخواست‌های هم‌جاوری استفاده شده است و قابل حذف نیست"
                    });

                _context.Set<Faaliat>().Remove(faaliat);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "فعالیت با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف فعالیت",
                    error = ex.Message
                });
            }
        }
        
    }
}