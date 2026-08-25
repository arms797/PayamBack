// Controllers/Edu/ReshtehController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Reshteh;
using PayamBack.Models.Edu;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReshtehController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string AllReshtehCacheKey = "AllReshtehList";

        public ReshtehController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه رشته‌ها
        // ============================================================
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList()
        {
            try
            {
                if (_cache.TryGetValue(AllReshtehCacheKey, out List<ReshtehListDto>? cachedData) && cachedData != null)
                {
                    return Ok(new { success = true, message = "لیست رشته‌ها دریافت شد", data = cachedData });
                }

                var reshtehs = await _context.Reshtehs
                    .OrderBy(r => r.OnvanReshte)
                    .Select(r => new ReshtehListDto
                    {
                        Id = r.Id,
                        GrooheAmoozeshiId = r.GrooheAmoozeshiId,
                        GrooheName = r.GrooheAmoozeshi != null ? r.GrooheAmoozeshi.OnvanGrooheAmoozeshi : null,
                        CodeMaghta = r.CodeMaghta,
                        Maghta = r.Maghta,
                        CodeReshte = r.CodeReshte,
                        OnvanReshte = r.OnvanReshte
                    })
                    .ToListAsync();

                _cache.Set(AllReshtehCacheKey, reshtehs, TimeSpan.FromHours(1));

                return Ok(new
                {
                    success = true,
                    message = "لیست رشته‌ها دریافت شد",
                    data = reshtehs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت رشته‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت رشته‌های یک گروه آموزشی خاص
        // ============================================================
        [HttpGet("by-groohe/{grooheId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByGroohe(int grooheId)
        {
            try
            {
                var grooheExists = await _context.GrooheAmoozeshis
                    .AnyAsync(g => g.Id == grooheId);

                if (!grooheExists)
                    return NotFound(new { success = false, message = "گروه آموزشی یافت نشد" });

                var reshtehs = await _context.Reshtehs
                    .Where(r => r.GrooheAmoozeshiId == grooheId)
                    .OrderBy(r => r.OnvanReshte)
                    .Select(r => new ReshtehListDto
                    {
                        Id = r.Id,
                        GrooheAmoozeshiId = r.GrooheAmoozeshiId,
                        CodeMaghta = r.CodeMaghta,
                        Maghta = r.Maghta,
                        CodeReshte = r.CodeReshte,
                        OnvanReshte = r.OnvanReshte
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "رشته‌های گروه آموزشی دریافت شد",
                    data = reshtehs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت رشته‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ دریافت یک رشته با شناسه
        // ============================================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var reshteh = await _context.Reshtehs
                    .Where(r => r.Id == id)
                    .Select(r => new ReshtehDetailDto
                    {
                        Id = r.Id,
                        GrooheAmoozeshiId = r.GrooheAmoozeshiId,
                        GrooheName = r.GrooheAmoozeshi != null ? r.GrooheAmoozeshi.OnvanGrooheAmoozeshi : null,
                        CodeMaghta = r.CodeMaghta,
                        Maghta = r.Maghta,
                        CodeReshte = r.CodeReshte,
                        OnvanReshte = r.OnvanReshte,
                        TermVorood = r.TermVorood,
                        TermEamal = r.TermEamal
                    })
                    .FirstOrDefaultAsync();

                if (reshteh == null)
                    return NotFound(new { success = false, message = "رشته یافت نشد" });

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات رشته دریافت شد",
                    data = reshteh
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات رشته",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ایجاد رشته جدید (نیاز به مجوز)
        // ============================================================
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ReshtehCreateDto dto)
        {
            try
            {
                var grooheExists = await _context.GrooheAmoozeshis
                    .AnyAsync(g => g.Id == dto.GrooheAmoozeshiId);

                if (!grooheExists)
                    return BadRequest(new { success = false, message = "گروه آموزشی یافت نشد" });

                var exists = await _context.Reshtehs
                    .AnyAsync(r => r.GrooheAmoozeshiId == dto.GrooheAmoozeshiId &&
                                   r.OnvanReshte == dto.OnvanReshte);

                if (exists)
                    return BadRequest(new { success = false, message = "این رشته قبلاً در این گروه ثبت شده است" });

                var reshteh = new Reshteh
                {
                    GrooheAmoozeshiId = dto.GrooheAmoozeshiId,
                    CodeMaghta = dto.CodeMaghta,
                    Maghta = dto.Maghta,
                    CodeReshte = dto.CodeReshte,
                    OnvanReshte = dto.OnvanReshte,
                    TermVorood = dto.TermVorood,
                    TermEamal = dto.TermEamal
                };

                await _context.Reshtehs.AddAsync(reshteh);
                await _context.SaveChangesAsync();

                // پاک کردن کش
                _cache.Remove(AllReshtehCacheKey);

                return Ok(new
                {
                    success = true,
                    message = "رشته با موفقیت ایجاد شد",
                    data = new { id = reshteh.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد رشته",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ ویرایش رشته (نیاز به مجوز)
        // ============================================================
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ReshtehUpdateDto dto)
        {
            try
            {
                var reshteh = await _context.Reshtehs
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reshteh == null)
                    return NotFound(new { success = false, message = "رشته یافت نشد" });

                if (dto.GrooheAmoozeshiId.HasValue)
                {
                    var grooheExists = await _context.GrooheAmoozeshis
                        .AnyAsync(g => g.Id == dto.GrooheAmoozeshiId.Value);

                    if (!grooheExists)
                        return BadRequest(new { success = false, message = "گروه آموزشی یافت نشد" });
                }

                // بررسی تکراری نبودن
                var targetGrooheId = dto.GrooheAmoozeshiId ?? reshteh.GrooheAmoozeshiId;
                if (!string.IsNullOrEmpty(dto.OnvanReshte) && dto.OnvanReshte != reshteh.OnvanReshte)
                {
                    var exists = await _context.Reshtehs
                        .AnyAsync(r => r.GrooheAmoozeshiId == targetGrooheId &&
                                       r.OnvanReshte == dto.OnvanReshte &&
                                       r.Id != id);

                    if (exists)
                        return BadRequest(new { success = false, message = "این رشته قبلاً در این گروه ثبت شده است" });
                }

                reshteh.GrooheAmoozeshiId = dto.GrooheAmoozeshiId ?? reshteh.GrooheAmoozeshiId;
                reshteh.CodeMaghta = dto.CodeMaghta ?? reshteh.CodeMaghta;
                reshteh.Maghta = dto.Maghta ?? reshteh.Maghta;
                reshteh.CodeReshte = dto.CodeReshte ?? reshteh.CodeReshte;
                reshteh.OnvanReshte = dto.OnvanReshte ?? reshteh.OnvanReshte;
                reshteh.TermVorood = dto.TermVorood ?? reshteh.TermVorood;
                reshteh.TermEamal = dto.TermEamal ?? reshteh.TermEamal;

                await _context.SaveChangesAsync();

                // پاک کردن کش
                _cache.Remove(AllReshtehCacheKey);

                return Ok(new
                {
                    success = true,
                    message = "رشته با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش رشته",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ حذف رشته (نیاز به مجوز)
        // ============================================================
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var reshteh = await _context.Reshtehs
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reshteh == null)
                    return NotFound(new { success = false, message = "رشته یافت نشد" });

                // بررسی استفاده شدن در دانشجویان
                var isUsed = await _context.Daneshjoos
                    .AnyAsync(d => d.ReshtehId == id);

                if (isUsed)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این رشته به دانشجویان متصل است و قابل حذف نیست"
                    });

                _context.Reshtehs.Remove(reshteh);
                await _context.SaveChangesAsync();

                // پاک کردن کش
                _cache.Remove(AllReshtehCacheKey);

                return Ok(new
                {
                    success = true,
                    message = "رشته با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف رشته",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ پاک کردن کش
        // ============================================================
        [HttpDelete("clear-cache")]
        [Authorize(Roles = "ادمین سامانه")]
        public IActionResult ClearCache()
        {
            _cache.Remove(AllReshtehCacheKey);
            return Ok(new { success = true, message = "کش رشته‌ها پاک شد" });
        }
    }
}