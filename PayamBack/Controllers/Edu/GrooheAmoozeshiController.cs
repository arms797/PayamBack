using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Edu;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrooheAmoozeshiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GrooheAmoozeshiController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه گروه‌های آموزشی (عمومی)
        // ============================================================
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var list = await _context.GrooheAmoozeshis
                    .OrderBy(g => g.CodeDaneshkade)
                    .ThenBy(g => g.CodeGrooheAmoozeshi)
                    .Select(g => new
                    {
                        g.Id,
                        g.CodeDaneshkade,
                        g.NaamDaneshkadeh,
                        g.CodeGrooheAmoozeshi,
                        g.OnvanGrooheAmoozeshi
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست گروه‌های آموزشی دریافت شد",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت گروه‌های آموزشی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت گروه‌های آموزشی بر اساس کد دانشکده (عمومی)
        // ============================================================
        [HttpGet("by-daneshkade/{codeDaneshkade}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDaneshkade(string codeDaneshkade)
        {
            try
            {
                if (string.IsNullOrEmpty(codeDaneshkade))
                    return BadRequest(new { success = false, message = "کد دانشکده وارد نشده است" });

                var list = await _context.GrooheAmoozeshis
                    .Where(g => g.CodeDaneshkade == codeDaneshkade)
                    .OrderBy(g => g.CodeGrooheAmoozeshi)
                    .Select(g => new
                    {
                        g.Id,
                        g.CodeDaneshkade,
                        g.NaamDaneshkadeh,
                        g.CodeGrooheAmoozeshi,
                        g.OnvanGrooheAmoozeshi
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "گروه‌های آموزشی دانشکده دریافت شد",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت گروه‌های آموزشی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ دریافت یک گروه آموزشی با شناسه (عمومی)
        // ============================================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _context.GrooheAmoozeshis
                    .Where(g => g.Id == id)
                    .Select(g => new
                    {
                        g.Id,
                        g.CodeDaneshkade,
                        g.NaamDaneshkadeh,
                        g.CodeGrooheAmoozeshi,
                        g.OnvanGrooheAmoozeshi
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                    return NotFound(new { success = false, message = "گروه آموزشی یافت نشد" });

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات گروه آموزشی دریافت شد",
                    data = item
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات گروه آموزشی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ دریافت لیست دانشکده‌های یکتا (عمومی)
        // ============================================================
        [HttpGet("daneshkade-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDaneshkadeList()
        {
            try
            {
                var list = await _context.GrooheAmoozeshis
                    .Where(g => g.CodeDaneshkade != null && g.NaamDaneshkadeh != null)
                    .GroupBy(g => new { g.CodeDaneshkade, g.NaamDaneshkadeh })
                    .Select(g => new
                    {
                        CodeDaneshkade = g.Key.CodeDaneshkade,
                        NaamDaneshkadeh = g.Key.NaamDaneshkadeh
                    })
                    .OrderBy(g => g.CodeDaneshkade)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست دانشکده‌ها دریافت شد",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست دانشکده‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ ایجاد گروه آموزشی جدید (نیاز به مجوز)
        // ============================================================
        [HttpPost("create")]
        [Authorize]  // ← فقط احراز هویت، مجوز توسط PermissionFilter بررسی می‌شود
        public async Task<IActionResult> Create([FromBody] GrooheAmoozeshiCreateDto dto)
        {
            try
            {
                var exists = await _context.GrooheAmoozeshis
                    .AnyAsync(g => g.CodeDaneshkade == dto.CodeDaneshkade &&
                                   g.CodeGrooheAmoozeshi == dto.CodeGrooheAmoozeshi);

                if (exists)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این گروه آموزشی قبلاً ثبت شده است"
                    });

                var item = new GrooheAmoozeshi
                {
                    CodeDaneshkade = dto.CodeDaneshkade,
                    NaamDaneshkadeh = dto.NaamDaneshkadeh,
                    CodeGrooheAmoozeshi = dto.CodeGrooheAmoozeshi,
                    OnvanGrooheAmoozeshi = dto.OnvanGrooheAmoozeshi
                };

                await _context.GrooheAmoozeshis.AddAsync(item);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "گروه آموزشی با موفقیت ایجاد شد",
                    data = new { id = item.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد گروه آموزشی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ ویرایش گروه آموزشی (نیاز به مجوز)
        // ============================================================
        [HttpPut("update/{id}")]
        [Authorize]  // ← فقط احراز هویت، مجوز توسط PermissionFilter بررسی می‌شود
        public async Task<IActionResult> Update(int id, [FromBody] GrooheAmoozeshiUpdateDto dto)
        {
            try
            {
                var item = await _context.GrooheAmoozeshis
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (item == null)
                    return NotFound(new { success = false, message = "گروه آموزشی یافت نشد" });

                var exists = await _context.GrooheAmoozeshis
                    .AnyAsync(g => g.Id != id &&
                                   g.CodeDaneshkade == (dto.CodeDaneshkade ?? item.CodeDaneshkade) &&
                                   g.CodeGrooheAmoozeshi == (dto.CodeGrooheAmoozeshi ?? item.CodeGrooheAmoozeshi));

                if (exists)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این گروه آموزشی قبلاً ثبت شده است"
                    });

                item.CodeDaneshkade = dto.CodeDaneshkade ?? item.CodeDaneshkade;
                item.NaamDaneshkadeh = dto.NaamDaneshkadeh ?? item.NaamDaneshkadeh;
                item.CodeGrooheAmoozeshi = dto.CodeGrooheAmoozeshi ?? item.CodeGrooheAmoozeshi;
                item.OnvanGrooheAmoozeshi = dto.OnvanGrooheAmoozeshi ?? item.OnvanGrooheAmoozeshi;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "گروه آموزشی با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش گروه آموزشی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ حذف گروه آموزشی (نیاز به مجوز)
        // ============================================================
        [HttpDelete("delete/{id}")]
        [Authorize]  // ← فقط احراز هویت، مجوز توسط PermissionFilter بررسی می‌شود
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.GrooheAmoozeshis
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (item == null)
                    return NotFound(new { success = false, message = "گروه آموزشی یافت نشد" });

                var isUsed = await _context.OstadMadraks
                    .AnyAsync(m => m.GrooheAmoozeshiId == id);

                if (isUsed)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این گروه آموزشی به مدارک استاد متصل است و قابل حذف نیست"
                    });

                _context.GrooheAmoozeshis.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "گروه آموزشی با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف گروه آموزشی",
                    error = ex.Message
                });
            }
        }
    }

    // ============================================================
    // DTOها
    // ============================================================

    public class GrooheAmoozeshiCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NaamDaneshkadeh { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string OnvanGrooheAmoozeshi { get; set; } = string.Empty;
    }

    public class GrooheAmoozeshiUpdateDto
    {
        [MaxLength(50)]
        public string? CodeDaneshkade { get; set; }

        [MaxLength(200)]
        public string? NaamDaneshkadeh { get; set; }

        [MaxLength(50)]
        public string? CodeGrooheAmoozeshi { get; set; }

        [MaxLength(200)]
        public string? OnvanGrooheAmoozeshi { get; set; }
    }
}