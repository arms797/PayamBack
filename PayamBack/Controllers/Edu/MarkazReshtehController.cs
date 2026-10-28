using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Edu.MarkazReshteh;
using PayamBack.Models.Edu;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarkazReshtehController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MarkazReshtehController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ لیست با فیلتر و صفحه‌بندی
        // ============================================================
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] string[]? maghtas = null,
            [FromQuery] string[]? daneshkades = null,
            [FromQuery] int[]? grooheAmoozeshiIds = null,
            [FromQuery] int[]? reshtehIds = null,
            [FromQuery] bool? vazeeyat = null,
            [FromQuery] string[]? ostans = null,
            [FromQuery] int[]? markazIds = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.MarkazReshtehs
                    .AsNoTracking()
                    .Include(mr => mr.Markaz)
                    .Include(mr => mr.Reshteh)
                        .ThenInclude(r => r.GrooheAmoozeshi)
                    .AsQueryable();

                // ---- جستجو: نام یا کد رشته ----
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    query = query.Where(mr =>
                        (mr.Reshteh != null && mr.Reshteh.OnvanReshte != null &&
                            mr.Reshteh.OnvanReshte.Contains(s)) ||
                        (mr.Reshteh != null && mr.Reshteh.CodeReshte != null &&
                            mr.Reshteh.CodeReshte.Contains(s)));
                }

                // ---- مقطع ----
                if (maghtas != null && maghtas.Length > 0)
                {
                    query = query.Where(mr =>
                        mr.Reshteh != null &&
                        mr.Reshteh.CodeMaghta != null &&
                        maghtas.Contains(mr.Reshteh.CodeMaghta));
                }

                // ---- دانشکده ----
                if (daneshkades != null && daneshkades.Length > 0)
                {
                    query = query.Where(mr =>
                        mr.Reshteh != null &&
                        mr.Reshteh.GrooheAmoozeshi != null &&
                        mr.Reshteh.GrooheAmoozeshi.CodeDaneshkade != null &&
                        daneshkades.Contains(mr.Reshteh.GrooheAmoozeshi.CodeDaneshkade));
                }

                // ---- گروه آموزشی ----
                if (grooheAmoozeshiIds != null && grooheAmoozeshiIds.Length > 0)
                {
                    query = query.Where(mr =>
                        mr.Reshteh != null &&
                        mr.Reshteh.GrooheAmoozeshiId.HasValue &&
                        grooheAmoozeshiIds.Contains(mr.Reshteh.GrooheAmoozeshiId.Value));
                }
                // 🔥 فیلتر رشته (reshtehIds)
                if (reshtehIds != null && reshtehIds.Length > 0)
                {
                    query = query.Where(mr => reshtehIds.Contains(mr.ReshtehId));
                }
                // ---- وضعیت ----
                if (vazeeyat.HasValue)
                {
                    query = query.Where(mr => mr.Vazeeat == vazeeyat.Value);
                }

                // ---- استان ----
                if (ostans != null && ostans.Length > 0)
                {
                    query = query.Where(mr =>
                        mr.Markaz != null &&
                        mr.Markaz.CodeOstan != null &&
                        ostans.Contains(mr.Markaz.CodeOstan));
                }

                // ---- مرکز ----
                if (markazIds != null && markazIds.Length > 0)
                {
                    query = query.Where(mr => markazIds.Contains(mr.MarkazId));
                }

                // ---- شمارش ----
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // ---- Projection سبک ----
                var data = await query
                    .OrderBy(mr => mr.Reshteh!.CodeReshte)
                    .ThenBy(mr => mr.Markaz!.NaamMarkaz)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(mr => new MarkazReshtehListDto
                    {
                        Id = mr.Id,
                        MarkazId = mr.MarkazId,
                        ReshtehId = mr.ReshtehId,

                        // رشته از بک‌اند
                        ReshtehName = mr.Reshteh != null ? mr.Reshteh.OnvanReshte : null,
                        CodeReshte = mr.Reshteh != null ? mr.Reshteh.CodeReshte : null,
                        CodeMaghta = mr.Reshteh != null ? mr.Reshteh.CodeMaghta : null,
                        Maghta = mr.Reshteh != null ? mr.Reshteh.Maghta : null,

                        // شناسه‌های Context
                        GrooheAmoozeshiId = mr.Reshteh != null ? mr.Reshteh.GrooheAmoozeshiId : null,
                        CodeDaneshkadeh = mr.Reshteh != null && mr.Reshteh.GrooheAmoozeshi != null
                            ? mr.Reshteh.GrooheAmoozeshi.CodeDaneshkade
                            : null,
                        CodeOstan = mr.Markaz != null ? mr.Markaz.CodeOstan : null,

                        // داده‌ی خود رکورد
                        TedadDaneshjoo = mr.TedadDaneshjoo,
                        Vazeeat = mr.Vazeeat
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست رشته‌های مرکز دریافت شد",
                    data,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages
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
        // 2️⃣ دریافت یک رکورد
        // ============================================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _context.MarkazReshtehs
                    .AsNoTracking()
                    .Include(mr => mr.Markaz)
                    .Include(mr => mr.Reshteh)
                        .ThenInclude(r => r.GrooheAmoozeshi)
                    .FirstOrDefaultAsync(mr => mr.Id == id);

                if (item == null)
                    return NotFound(new { success = false, message = "رکورد یافت نشد" });

                var dto = new MarkazReshtehDetailDto
                {
                    Id = item.Id,

                    MarkazId = item.MarkazId,
                    MarkazName = item.Markaz?.NaamMarkaz,
                    MarkazCode = item.Markaz?.CodeMarkaz,
                    CodeOstan = item.Markaz?.CodeOstan,
                    OstanName = item.Markaz?.NaamOstan,

                    ReshtehId = item.ReshtehId,
                    ReshtehName = item.Reshteh?.OnvanReshte,
                    CodeReshte = item.Reshteh?.CodeReshte,
                    CodeMaghta = item.Reshteh?.CodeMaghta,
                    Maghta = item.Reshteh?.Maghta,

                    GrooheAmoozeshiId = item.Reshteh?.GrooheAmoozeshiId,
                    GrooheAmoozeshiName = item.Reshteh?.GrooheAmoozeshi?.OnvanGrooheAmoozeshi,
                    CodeDaneshkadeh = item.Reshteh?.GrooheAmoozeshi?.CodeDaneshkade,
                    DaneshkadehName = item.Reshteh?.GrooheAmoozeshi?.NaamDaneshkadeh,

                    TedadDaneshjoo = item.TedadDaneshjoo,
                    Vazeeat = item.Vazeeat
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت رکورد",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد
        // ============================================================
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] MarkazReshtehCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "اطلاعات ورودی نامعتبر است",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var markazExists = await _context.Markazes
                    .AnyAsync(m => m.Id == dto.MarkazId);

                if (!markazExists)
                    return BadRequest(new { success = false, message = "مرکز یافت نشد" });

                var reshtehExists = await _context.Reshtehs
                    .AnyAsync(r => r.Id == dto.ReshtehId);

                if (!reshtehExists)
                    return BadRequest(new { success = false, message = "رشته یافت نشد" });

                var duplicate = await _context.MarkazReshtehs
                    .AnyAsync(mr => mr.MarkazId == dto.MarkazId && mr.ReshtehId == dto.ReshtehId);

                if (duplicate)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این رشته قبلاً برای این مرکز ثبت شده است"
                    });

                var entity = new MarkazReshteh
                {
                    MarkazId = dto.MarkazId,
                    ReshtehId = dto.ReshtehId,
                    TedadDaneshjoo = dto.TedadDaneshjoo,
                    Vazeeat = dto.Vazeeat
                };

                await _context.MarkazReshtehs.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "رشته با موفقیت به مرکز اضافه شد",
                    data = new { id = entity.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد رکورد",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش
        // ============================================================
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] MarkazReshtehUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "اطلاعات ورودی نامعتبر است",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var entity = await _context.MarkazReshtehs
                    .FirstOrDefaultAsync(mr => mr.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "رکورد یافت نشد" });

                if (dto.TedadDaneshjoo.HasValue)
                    entity.TedadDaneshjoo = dto.TedadDaneshjoo;

                if (dto.Vazeeat.HasValue)
                    entity.Vazeeat = dto.Vazeeat.Value;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "رکورد با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش رکورد",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ تغییر وضعیت
        // ============================================================
        [HttpPatch("toggle-status/{id}")]
        [Authorize]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var entity = await _context.MarkazReshtehs
                    .FirstOrDefaultAsync(mr => mr.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "رکورد یافت نشد" });

                entity.Vazeeat = !entity.Vazeeat;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = entity.Vazeeat ? "رشته فعال شد" : "رشته غیرفعال شد",
                    data = new { vazeeat = entity.Vazeeat }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت",
                    error = ex.Message
                });
            }
        }
    }
}
