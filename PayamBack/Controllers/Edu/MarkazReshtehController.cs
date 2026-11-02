using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Edu.MarkazReshteh;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Services.Interfaces;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarkazReshtehController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public MarkazReshtehController(AppDbContext context,ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
            [FromQuery] int[]? level=null,
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
                // ---- مرکز ----
                if (markazIds != null && markazIds.Length > 0)
                {
                    query = query.Where(mr => markazIds.Contains(mr.MarkazId));
                }

                // ---- سطح (پیش‌فرض 4) ----
                if (level != null && level.Length > 0)
                {
                    query = query.Where(mr => mr.Level.HasValue && level.Contains(mr.Level.Value));
                }
                else
                {
                    // 🔥 پیش‌فرض: فقط Level = 4
                    query = query.Where(mr => mr.Level == 4);
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
                        Level = mr.Level,
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
        // 3️⃣ ایجاد (لیستی) — با منطق نقش و سطح
        //    کلید یکتا: (MarkazId, ReshtehId, Level)
        //    نقش ۱ = همه‌ی نقش‌ها را می‌تواند بازی کند (بر اساس Level DTO)
        // ============================================================
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] List<MarkazReshtehCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { success = false, message = "لیست خالی است" });

            try
            {
                // ============================================================
                // 🔥 ۱. بررسی نقش کاربر
                // ============================================================
                var (user, role, userMarkaz, codeRole) = await _currentUser.GetCurrentUserInfoAsync();

                if (user == null || !codeRole.HasValue)
                    return Unauthorized(new { success = false, message = "اطلاعات کاربری نامعتبر است" });

                // 🔥 لول‌های مجاز بر اساس نقش
                var allowedLevels = codeRole.Value switch
                {
                    1 => new[] { 1, 2, 3, 4 },   // ادمین سامانه: همه
                    2 => new[] { 2 },             // سازمان مرکزی
                    3 => new[] { 3 },             // استان
                    4 => new[] { 4 },             // مرکز
                    _ => Array.Empty<int>()
                };

                if (allowedLevels.Length == 0)
                    return Forbid();

                var userOstanCode = userMarkaz?.CodeOstan;
                var userMarkazId = userMarkaz?.Id;

                // ============================================================
                // ۲. پردازش هر DTO
                // ============================================================
                var rejected = new List<RejectedMarkazReshtehRowDto>();
                var toInsert = new List<MarkazReshteh>();

                // جلوگیری از تکرار در همون درخواست — با Level
                var processedKeys = new HashSet<(int, int, int)>();

                foreach (var dto in dtos)
                {
                    try
                    {
                        // ---- ۲.۱) لول درخواستی ----
                        var requestedLevel = dto.Level ?? 4;

                        // ---- ۲.۲) بررسی مجاز بودن سطح ----
                        if (!allowedLevels.Contains(requestedLevel))
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = 0,
                                CodeMarkaz = dto.MarkazId.ToString(),
                                CodeReshte = dto.ReshtehId.ToString(),
                                Reason = $"شما مجاز به ایجاد رشته در سطح {requestedLevel} نیستید (نقش شما فقط سطوح {string.Join(", ", allowedLevels)} را مجاز می‌کند)"
                            });
                            continue;
                        }

                        // ---- ۲.۳) بررسی وجود مرکز ----
                        var markaz = await _context.Markazes
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == dto.MarkazId);

                        if (markaz == null)
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = 0,
                                CodeMarkaz = dto.MarkazId.ToString(),
                                CodeReshte = dto.ReshtehId.ToString(),
                                Reason = "مرکز یافت نشد"
                            });
                            continue;
                        }

                        // ---- ۲.۴) بررسی وجود رشته ----
                        var reshtehExists = await _context.Reshtehs
                            .AnyAsync(r => r.Id == dto.ReshtehId);

                        if (!reshtehExists)
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = 0,
                                CodeMarkaz = dto.MarkazId.ToString(),
                                CodeReshte = dto.ReshtehId.ToString(),
                                Reason = "رشته یافت نشد"
                            });
                            continue;
                        }

                        // ============================================================
                        // 🔥 ۲.۵) اعتبارسنجی دسترسی بر اساس نقش
                        // ============================================================

                        // ---- نقش ۴: فقط مرکز خودش ----
                        if (codeRole.Value == 4)
                        {
                            if (!userMarkazId.HasValue)
                            {
                                rejected.Add(new RejectedMarkazReshtehRowDto
                                {
                                    RowNumber = 0,
                                    CodeMarkaz = dto.MarkazId.ToString(),
                                    CodeReshte = dto.ReshtehId.ToString(),
                                    Reason = "مرکز کاربری شما مشخص نیست"
                                });
                                continue;
                            }

                            if (dto.MarkazId != userMarkazId.Value)
                            {
                                rejected.Add(new RejectedMarkazReshtehRowDto
                                {
                                    RowNumber = 0,
                                    CodeMarkaz = dto.MarkazId.ToString(),
                                    CodeReshte = dto.ReshtehId.ToString(),
                                    Reason = "شما فقط می‌توانید برای مرکز خود رشته اضافه کنید"
                                });
                                continue;
                            }
                        }

                        // ---- نقش ۳: فقط مراکز استان خودش ----
                        if (codeRole.Value == 3)
                        {
                            if (string.IsNullOrEmpty(userOstanCode))
                            {
                                rejected.Add(new RejectedMarkazReshtehRowDto
                                {
                                    RowNumber = 0,
                                    CodeMarkaz = dto.MarkazId.ToString(),
                                    CodeReshte = dto.ReshtehId.ToString(),
                                    Reason = "کد استان کاربری شما مشخص نیست"
                                });
                                continue;
                            }

                            if (markaz.CodeOstan != userOstanCode)
                            {
                                rejected.Add(new RejectedMarkazReshtehRowDto
                                {
                                    RowNumber = 0,
                                    CodeMarkaz = dto.MarkazId.ToString(),
                                    CodeReshte = dto.ReshtehId.ToString(),
                                    Reason = "شما فقط می‌توانید برای مراکز استان خود رشته اضافه کنید"
                                });
                                continue;
                            }
                        }

                        // ---- نقش ۱: هیچ محدودیت مرکز/استانی ندارد ----

                        // ============================================================
                        // 🔥 ۲.۶) کلید یکتا: (MarkazId, ReshtehId, Level)
                        // ============================================================
                        var mrKey = (dto.MarkazId, dto.ReshtehId, requestedLevel);

                        if (!processedKeys.Add(mrKey))
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = 0,
                                CodeMarkaz = dto.MarkazId.ToString(),
                                CodeReshte = dto.ReshtehId.ToString(),
                                Reason = $"این ترکیب (مرکز + رشته + سطح {requestedLevel}) در درخواست تکراری است"
                            });
                            continue;
                        }

                        var duplicate = await _context.MarkazReshtehs
                            .AnyAsync(mr =>
                                mr.MarkazId == dto.MarkazId &&
                                mr.ReshtehId == dto.ReshtehId &&
                                mr.Level == requestedLevel);

                        if (duplicate)
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = 0,
                                CodeMarkaz = dto.MarkazId.ToString(),
                                CodeReshte = dto.ReshtehId.ToString(),
                                Reason = $"این رشته با سطح {requestedLevel} قبلاً برای این مرکز ثبت شده است"
                            });
                            continue;
                        }

                        // ============================================================
                        // 🔥 ۲.۷) محاسبه‌ی TedadDaneshjoo
                        // ============================================================
                        int? finalTedad = dto.TedadDaneshjoo;

                        // ═══════════════════════════════════════════════════════════
                        // حالت ۱: نقش ۲ (سازمان مرکزی) — جمع کل همه‌ی مراکز
                        // ═══════════════════════════════════════════════════════════
                        if (codeRole.Value == 2 && requestedLevel == 2)
                        {
                            var sumTedad = await _context.MarkazReshtehs
                                .Where(mr =>
                                    mr.ReshtehId == dto.ReshtehId &&
                                    mr.Level == 4)   // فقط رشته‌های عادی
                                .SumAsync(mr => (int?)mr.TedadDaneshjoo) ?? 0;

                            finalTedad = sumTedad;
                        }
                        // ═══════════════════════════════════════════════════════════
                        // حالت ۲: نقش ۳ (استان) — جمع مراکز استان کاربر
                        // ═══════════════════════════════════════════════════════════
                        else if (codeRole.Value == 3 && requestedLevel == 3)
                        {
                            var sumTedad = await _context.MarkazReshtehs
                                .Where(mr =>
                                    mr.ReshtehId == dto.ReshtehId &&
                                    mr.Level == 4 &&
                                    mr.Markaz != null &&
                                    mr.Markaz.CodeOstan == userOstanCode)
                                .SumAsync(mr => (int?)mr.TedadDaneshjoo) ?? 0;

                            finalTedad = sumTedad;
                        }
                        // ═══════════════════════════════════════════════════════════
                        // حالت ۳: نقش ۱ (ادمین سامانه) — بر اساس Level DTO
                        // ═══════════════════════════════════════════════════════════
                        else if (codeRole.Value == 1)
                        {
                            if (requestedLevel == 2)
                            {
                                // مثل نقش ۲ — جمع کل همه‌ی مراکز
                                var sumTedad = await _context.MarkazReshtehs
                                    .Where(mr =>
                                        mr.ReshtehId == dto.ReshtehId &&
                                        mr.Level == 4)
                                    .SumAsync(mr => (int?)mr.TedadDaneshjoo) ?? 0;

                                finalTedad = sumTedad;
                            }
                            else if (requestedLevel == 3)
                            {
                                // مثل نقش ۳ — جمع مراکز استانِ مرکزِ DTO
                                var dtoMarkazOstan = markaz.CodeOstan;

                                if (string.IsNullOrEmpty(dtoMarkazOstan))
                                {
                                    rejected.Add(new RejectedMarkazReshtehRowDto
                                    {
                                        RowNumber = 0,
                                        CodeMarkaz = dto.MarkazId.ToString(),
                                        CodeReshte = dto.ReshtehId.ToString(),
                                        Reason = "کد استان مرکز مشخص نیست"
                                    });
                                    continue;
                                }

                                var sumTedad = await _context.MarkazReshtehs
                                    .Where(mr =>
                                        mr.ReshtehId == dto.ReshtehId &&
                                        mr.Level == 4 &&
                                        mr.Markaz != null &&
                                        mr.Markaz.CodeOstan == dtoMarkazOstan)
                                    .SumAsync(mr => (int?)mr.TedadDaneshjoo) ?? 0;

                                finalTedad = sumTedad;
                            }
                            else if (requestedLevel == 4)
                            {
                                // مثل نقش ۴ — مستقیم از DTO
                                finalTedad = dto.TedadDaneshjoo;
                            }
                            // Level = 1: مستقیم از DTO (نادر)
                        }
                        // ═══════════════════════════════════════════════════════════
                        // حالت ۴: نقش ۴ (مرکز) — مستقیم از DTO
                        // ═══════════════════════════════════════════════════════════
                        // (finalTedad = dto.TedadDaneshjoo از قبل ست شده)

                        // ---- ۲.۸) ساخت entity ----
                        var entity = new MarkazReshteh
                        {
                            MarkazId = dto.MarkazId,
                            ReshtehId = dto.ReshtehId,
                            TedadDaneshjoo = finalTedad,
                            Vazeeat = dto.Vazeeat,
                            Level = requestedLevel
                        };

                        toInsert.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        rejected.Add(new RejectedMarkazReshtehRowDto
                        {
                            RowNumber = 0,
                            CodeMarkaz = dto.MarkazId.ToString(),
                            CodeReshte = dto.ReshtehId.ToString(),
                            Reason = $"خطای غیرمنتظره: {ex.Message}"
                        });
                    }
                }

                // ============================================================
                // ۳. ذخیره در DB
                // ============================================================
                int inserted = 0;
                if (toInsert.Any())
                {
                    await _context.MarkazReshtehs.AddRangeAsync(toInsert);
                    await _context.SaveChangesAsync();
                    inserted = toInsert.Count;
                }

                // ============================================================
                // ۴. خروجی
                // ============================================================
                return Ok(new
                {
                    success = true,
                    message = $"{inserted} رکورد با موفقیت اضافه شد، {rejected.Count} رکورد رد شد.",
                    data = new
                    {
                        inserted,
                        rejected = rejected.Count,
                        total = dtos.Count
                    },
                    rejectedRows = rejected
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در پردازش درخواست",
                    error = ex.Message
                });
            }
        }
        /*
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
                    Vazeeat = dto.Vazeeat,
                    Level = dto.Level ?? 4   // 🔥 اگه null بود، ۴
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
        */

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

        // ============================================================
        // 6️⃣ BulkUpload از فایل اکسل
        // ============================================================
        [HttpPost("bulk-upload")]
        [Authorize]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "فایلی انتخاب نشده است" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { success = false, message = "فقط فایل‌های Excel مجاز هستند" });

            try
            {
                // ============================================================
                // ۱. خواندن اکسل
                // ============================================================
                List<MarkazReshtehImportRow> rows;
                try
                {
                    using var stream = file.OpenReadStream();
                    rows = ParseMarkazReshtehExcel(stream);
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در خواندن فایل اکسل. لطفاً فرمت فایل را بررسی کنید",
                        error = ex.Message
                    });
                }

                if (rows.Count == 0)
                    return BadRequest(new { success = false, message = "فایل خالی است" });

                // ============================================================
                // ۲. بارگذاری مراکز (یک بار)
                // ============================================================
                Dictionary<string, int> markazLookup;
                try
                {
                    var allMarkazes = await _context.Markazes
                        .AsNoTracking()
                        .Select(m => new { m.Id, m.CodeMarkaz })
                        .ToListAsync();

                    markazLookup = allMarkazes
                        .Where(m => !string.IsNullOrEmpty(m.CodeMarkaz))
                        .GroupBy(m => m.CodeMarkaz!.Trim())
                        .ToDictionary(g => g.Key, g => g.First().Id);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در بارگذاری مراکز از دیتابیس",
                        error = ex.Message
                    });
                }

                // ============================================================
                // ۳. بارگذاری رشته‌ها (یک بار)
                // ============================================================
                Dictionary<string, int> reshtehLookup;
                try
                {
                    var allReshtehs = await _context.Reshtehs
                        .AsNoTracking()
                        .Select(r => new
                        {
                            r.Id,
                            r.CodeMaghta,
                            r.CodeReshte,
                            r.TermVorood,
                            r.TermEamal
                        })
                        .ToListAsync();

                    reshtehLookup = allReshtehs
                        .Where(r => !string.IsNullOrEmpty(r.CodeReshte))
                        .GroupBy(r => MakeReshtehKey(r.CodeMaghta, r.CodeReshte, r.TermVorood, r.TermEamal))
                        .ToDictionary(g => g.Key, g => g.First().Id);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در بارگذاری رشته‌ها از دیتابیس",
                        error = ex.Message
                    });
                }

                // ============================================================
                // ۴. بارگذاری MarkazReshteh ها (یک بار)
                //    🔥 فقط رکوردهای Level = 4 (رشته‌های عادی)
                // ============================================================
                List<MarkazReshteh> allMarkazReshtehs;
                Dictionary<(int, int), MarkazReshteh> markazReshtehLookup;
                try
                {
                    allMarkazReshtehs = await _context.MarkazReshtehs
                        .Where(mr => mr.Level == 4)   // 🔥 فقط Level = 4
                        .ToListAsync();

                    markazReshtehLookup = allMarkazReshtehs
                        .GroupBy(mr => (mr.MarkazId, mr.ReshtehId))
                        .ToDictionary(g => g.Key, g => g.First());
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در بارگذاری رشته‌های مراکز از دیتابیس",
                        error = ex.Message
                    });
                }

                // ============================================================
                // ۵. پردازش ردیف‌ها
                // ============================================================
                var rejected = new List<RejectedMarkazReshtehRowDto>();
                var markazIdsInFile = new HashSet<int>();
                var markazReshtehKeysInFile = new HashSet<(int, int)>();
                var processedKeys = new HashSet<(int, int)>();

                int inserted = 0;
                int updated = 0;
                int skipped = 0;

                foreach (var row in rows)
                {
                    try
                    {
                        // ---- پیدا کردن مرکز ----
                        var codeMarkaz = row.CodeMarkaz?.Trim();
                        if (string.IsNullOrEmpty(codeMarkaz) || !markazLookup.TryGetValue(codeMarkaz, out var markazId))
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = row.RowNumber,
                                CodeOstan = row.CodeOstan,
                                CodeMarkaz = row.CodeMarkaz,
                                CodeMaghta = row.CodeMaghta,
                                CodeReshte = row.CodeReshte,
                                TermVorood = row.TermVorood,
                                TermEamal = row.TermEamal,
                                Reason = $"مرکز با کد '{row.CodeMarkaz}' یافت نشد"
                            });
                            continue;
                        }

                        // ---- پیدا کردن رشته ----
                        var reshtehKey = MakeReshtehKey(row.CodeMaghta, row.CodeReshte, row.TermVorood, row.TermEamal);
                        if (!reshtehLookup.TryGetValue(reshtehKey, out var reshtehId))
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = row.RowNumber,
                                CodeOstan = row.CodeOstan,
                                CodeMarkaz = row.CodeMarkaz,
                                CodeMaghta = row.CodeMaghta,
                                CodeReshte = row.CodeReshte,
                                TermVorood = row.TermVorood,
                                TermEamal = row.TermEamal,
                                Reason = "رشته با این کلید (کد مقطع + کد رشته + ترم ورود + ترم اعمال) یافت نشد"
                            });
                            continue;
                        }

                        // ---- اعتبارسنجی TedadDaneshjoo ----
                        if (!row.TedadDaneshjoo.HasValue || row.TedadDaneshjoo.Value <= 0)
                        {
                            rejected.Add(new RejectedMarkazReshtehRowDto
                            {
                                RowNumber = row.RowNumber,
                                CodeOstan = row.CodeOstan,
                                CodeMarkaz = row.CodeMarkaz,
                                CodeMaghta = row.CodeMaghta,
                                CodeReshte = row.CodeReshte,
                                TermVorood = row.TermVorood,
                                TermEamal = row.TermEamal,
                                Reason = "تعداد دانشجو باید بزرگ‌تر از ۰ باشد"
                            });
                            continue;
                        }

                        // ---- کلید نهایی ----
                        var mrKey = (markazId, reshtehId);

                        markazIdsInFile.Add(markazId);
                        markazReshtehKeysInFile.Add(mrKey);

                        if (!processedKeys.Add(mrKey))
                        {
                            skipped++;
                            continue;
                        }

                        // ---- Upsert ----
                        if (markazReshtehLookup.TryGetValue(mrKey, out var existing))
                        {
                            // 🔥 UPDATE: آپدیت + اطمینان از Level = 4
                            existing.TedadDaneshjoo = row.TedadDaneshjoo.Value;
                            existing.Vazeeat = true;
                            existing.Level = 4;   // 🔥 اگه قبلاً غیر ۴ بود، ۴ بشه
                            updated++;
                        }
                        else
                        {
                            // 🔥 INSERT: همیشه Level = 4
                            var newMr = new MarkazReshteh
                            {
                                MarkazId = markazId,
                                ReshtehId = reshtehId,
                                TedadDaneshjoo = row.TedadDaneshjoo.Value,
                                Vazeeat = true,
                                Level = 4   // 🔥 همیشه ۴
                            };
                            await _context.MarkazReshtehs.AddAsync(newMr);
                            markazReshtehLookup[mrKey] = newMr;
                            inserted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // خطای یک ردیف، بقیه رو متوقف نمی‌کنه
                        rejected.Add(new RejectedMarkazReshtehRowDto
                        {
                            RowNumber = row.RowNumber,
                            CodeOstan = row.CodeOstan,
                            CodeMarkaz = row.CodeMarkaz,
                            CodeMaghta = row.CodeMaghta,
                            CodeReshte = row.CodeReshte,
                            TermVorood = row.TermVorood,
                            TermEamal = row.TermEamal,
                            Reason = $"خطای غیرمنتظره: {ex.Message}"
                        });
                        continue;
                    }
                }
               
                // ============================================================
                // ۶. غیرفعال‌سازی (فقط مراکز موجود در فایل) + جمع‌آوری برای گزارش
                // ============================================================
                int deactivated = 0;
                var deactivatedRows = new List<DeactivatedMarkazReshtehDto>();
                try
                {
                    var toDeactivate = allMarkazReshtehs
                        .Where(mr =>
                            mr.Level == 4 &&   // 🔥 فقط Level = 4
                            markazIdsInFile.Contains(mr.MarkazId) &&
                            mr.Vazeeat != false &&
                            !markazReshtehKeysInFile.Contains((mr.MarkazId, mr.ReshtehId)))
                        .ToList();

                    // 🔥 برای گزارش، نام مرکز و رشته رو لازم داریم
                    var markazIdSet = toDeactivate.Select(x => x.MarkazId).Distinct().ToList();
                    var reshtehIdSet = toDeactivate.Select(x => x.ReshtehId).Distinct().ToList();

                    var markazNames = await _context.Markazes
                        .AsNoTracking()
                        .Where(m => markazIdSet.Contains(m.Id))
                        .Select(m => new { m.Id, m.CodeMarkaz, m.NaamMarkaz })
                        .ToDictionaryAsync(m => m.Id);

                    var reshtehNames = await _context.Reshtehs
                        .AsNoTracking()
                        .Where(r => reshtehIdSet.Contains(r.Id))
                        .Select(r => new
                        {
                            r.Id,
                            r.CodeReshte,
                            r.OnvanReshte,
                            r.CodeMaghta,
                            r.TermVorood,
                            r.TermEamal
                        })
                        .ToDictionaryAsync(r => r.Id);

                    foreach (var mr in toDeactivate)
                    {
                        mr.Vazeeat = false;
                        deactivated++;

                        markazNames.TryGetValue(mr.MarkazId, out var markaz);
                        reshtehNames.TryGetValue(mr.ReshtehId, out var reshteh);

                        deactivatedRows.Add(new DeactivatedMarkazReshtehDto
                        {
                            MarkazReshtehId = mr.Id,
                            CodeMarkaz = markaz?.CodeMarkaz,
                            NaamMarkaz = markaz?.NaamMarkaz,
                            CodeReshte = reshteh?.CodeReshte,
                            OnvanReshte = reshteh?.OnvanReshte,
                            CodeMaghta = reshteh?.CodeMaghta,
                            TermVorood = reshteh?.TermVorood,
                            TermEamal = reshteh?.TermEamal,
                            TedadDaneshjoo = mr.TedadDaneshjoo
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در غیرفعال‌سازی رکوردهای قدیمی",
                        error = ex.Message,
                        data = new { inserted, updated, skipped, rejected = rejected.Count }
                    });
                }

                // ============================================================
                // ۷. ذخیره‌ی نهایی
                // ============================================================
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطا در ذخیره‌سازی به دیتابیس",
                        error = innerMsg
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "خطای نامشخص در ذخیره‌سازی",
                        error = ex.Message
                    });
                }

                // ============================================================
                // ۸. خروجی
                // ============================================================
                return Ok(new
                {
                    success = true,
                    message = $"{inserted} رکورد جدید، {updated} رکورد آپدیت، " +
                              $"{deactivated} رکورد غیرفعال، {skipped} تکراری، " +
                              $"{rejected.Count} رد شده.",
                    data = new
                    {
                        inserted,
                        updated,
                        deactivated,
                        skipped,
                        rejected = rejected.Count,
                        total = rows.Count
                    },
                    rejectedRows = rejected,
                    deactivatedRows   // 🔥 جدید
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطای کلی در پردازش فایل اکسل",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // Helper: ساخت کلید رشته
        // ============================================================
        private static string MakeReshtehKey(string? codeMaghta, string? codeReshte, string? termVorood, string? termEamal)
        {
            return $"{(codeMaghta ?? "").Trim()}|{(codeReshte ?? "").Trim()}|{(termVorood ?? "").Trim()}|{(termEamal ?? "").Trim()}";
        }

        // ============================================================
        // Parse Excel
        // ============================================================
        private List<MarkazReshtehImportRow> ParseMarkazReshtehExcel(Stream stream)
        {
            var rows = new List<MarkazReshtehImportRow>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var importRow = new MarkazReshtehImportRow
                    {
                        RowNumber = row,
                        CodeOstan = GetCellString(worksheet, row, 1),
                        CodeMarkaz = GetCellString(worksheet, row, 2),
                        CodeMaghta = GetCellString(worksheet, row, 3),
                        CodeReshte = GetCellString(worksheet, row, 4),
                        TermVorood = GetCellString(worksheet, row, 5),
                        TermEamal = GetCellString(worksheet, row, 6),
                        TedadDaneshjoo = GetCellInt(worksheet, row, 7)
                    };

                    if (!string.IsNullOrWhiteSpace(importRow.CodeMarkaz) ||
                        !string.IsNullOrWhiteSpace(importRow.CodeReshte))
                    {
                        rows.Add(importRow);
                    }
                }
                catch
                {
                    // خطا در یک ردیف، بقیه رو متوقف نکن
                    continue;
                }
            }

            return rows;
        }

        // ============================================================
        // Helpers برای خواندن Cell
        // ============================================================
        private string? GetCellString(IXLWorksheet ws, int row, int col)
        {
            try
            {
                var cell = ws.Cell(row, col);
                if (cell.IsEmpty()) return null;
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
            catch
            {
                return null;
            }
        }

        private int? GetCellInt(IXLWorksheet ws, int row, int col)
        {
            try
            {
                var cell = ws.Cell(row, col);
                if (cell.IsEmpty()) return null;
                if (cell.TryGetValue<int>(out var value)) return value;
                return null;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // 7️⃣ فیلتر رشته‌ها (مودال Adaptive)
        // ============================================================
        [HttpPost("filter-reshtehs")]
        [Authorize]
        public async Task<IActionResult> FilterReshtehs([FromBody] MarkazReshtehFilterDto dto)
        {
            try
            {
                var (user, role, userMarkaz, codeRole) = await _currentUser.GetCurrentUserInfoAsync();
                if (user == null || !codeRole.HasValue)
                    return Unauthorized(new { success = false, message = "اطلاعات کاربری نامعتبر است" });

                var allowedLevels = codeRole.Value switch
                {
                    1 => new[] { 2, 3, 4 },
                    2 => new[] { 2 },
                    3 => new[] { 3 },
                    4 => new[] { 4 },
                    _ => Array.Empty<int>()
                };

                if (!allowedLevels.Contains(dto.Level))
                    return Forbid();

                var markaz = await _context.Markazes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.MarkazId);

                if (markaz == null)
                    return BadRequest(new { success = false, message = "مرکز یافت نشد" });

                if (codeRole.Value == 4 && userMarkaz?.Id != dto.MarkazId)
                    return Forbid();

                if (codeRole.Value == 3 && markaz.CodeOstan != userMarkaz?.CodeOstan)
                    return Forbid();

                var query = _context.Reshtehs
                    .AsNoTracking()
                    .Include(r => r.GrooheAmoozeshi)
                    .AsQueryable();

                if (dto.Maghtas != null && dto.Maghtas.Length > 0)
                    query = query.Where(r => r.CodeMaghta != null && dto.Maghtas.Contains(r.CodeMaghta));

                if (dto.Daneshkades != null && dto.Daneshkades.Length > 0)
                    query = query.Where(r =>
                        r.GrooheAmoozeshi != null &&
                        r.GrooheAmoozeshi.CodeDaneshkade != null &&
                        dto.Daneshkades.Contains(r.GrooheAmoozeshi.CodeDaneshkade));

                if (dto.GrooheAmoozeshiIds != null && dto.GrooheAmoozeshiIds.Length > 0)
                    query = query.Where(r =>
                        r.GrooheAmoozeshiId.HasValue &&
                        dto.GrooheAmoozeshiIds.Contains(r.GrooheAmoozeshiId.Value));

                if (!string.IsNullOrWhiteSpace(dto.Search))
                {
                    var s = dto.Search.Trim();
                    query = query.Where(r =>
                        (r.CodeReshte != null && r.CodeReshte.Contains(s)) ||
                        (r.OnvanReshte != null && r.OnvanReshte.Contains(s)));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / dto.PageSize);

                var reshtehs = await query
                    .OrderBy(r => r.OnvanReshte)
                    .Skip((dto.Page - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.CodeReshte,
                        r.OnvanReshte,
                        r.CodeMaghta,
                        r.Maghta,
                        r.GrooheAmoozeshiId,
                        GrooheAmoozeshiName = r.GrooheAmoozeshi != null ? r.GrooheAmoozeshi.OnvanGrooheAmoozeshi : null,
                        CodeDaneshkadeh = r.GrooheAmoozeshi != null ? r.GrooheAmoozeshi.CodeDaneshkade : null,
                        DaneshkadehName = r.GrooheAmoozeshi != null ? r.GrooheAmoozeshi.NaamDaneshkadeh : null
                    })
                    .ToListAsync();

                var reshtehIds = reshtehs.Select(r => r.Id).ToList();

                var existingAssigned = await _context.MarkazReshtehs
                    .AsNoTracking()
                    .Where(mr =>
                        mr.MarkazId == dto.MarkazId &&
                        mr.Level == dto.Level &&
                        reshtehIds.Contains(mr.ReshtehId))
                    .Select(mr => new { mr.ReshtehId, mr.Id })
                    .ToDictionaryAsync(x => x.ReshtehId, x => x.Id);

                var calculatedTedads = await CalculateTedadsAsync(dto.Level, codeRole.Value, reshtehIds, userMarkaz);

                var data = reshtehs.Select(r => new MarkazReshtehFilteredItemDto
                {
                    ReshtehId = r.Id,
                    CodeReshte = r.CodeReshte,
                    OnvanReshte = r.OnvanReshte,
                    CodeMaghta = r.CodeMaghta,
                    Maghta = r.Maghta,
                    GrooheAmoozeshiId = r.GrooheAmoozeshiId,
                    GrooheAmoozeshiName = r.GrooheAmoozeshiName,
                    CodeDaneshkadeh = r.CodeDaneshkadeh,
                    DaneshkadehName = r.DaneshkadehName,
                    CalculatedTedad = calculatedTedads.GetValueOrDefault(r.Id),
                    AlreadyAssigned = existingAssigned.ContainsKey(r.Id),
                    ExistingMarkazReshtehId = existingAssigned.GetValueOrDefault(r.Id)
                }).ToList();

                return Ok(new
                {
                    success = true,
                    message = "لیست رشته‌ها دریافت شد",
                    data,
                    pagination = new { page = dto.Page, pageSize = dto.PageSize, totalCount, totalPages }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در فیلتر رشته‌ها", error = ex.Message });
            }
        }

        // ============================================================
        // Helper: محاسبه‌ی TedadDaneshjoo
        // ============================================================
        private async Task<Dictionary<int, int?>> CalculateTedadsAsync(
            int level,
            int codeRole,
            List<int> reshtehIds,
            Markaz? userMarkaz)
        {
            var result = new Dictionary<int, int?>();

            // نقش 4 یا نقش 1 + Level 4: دستی
            if (codeRole == 4 || (codeRole == 1 && level == 4))
                return result;

            // نقش 3 یا نقش 1 + Level 3: جمع استان
            if (codeRole == 3 || (codeRole == 1 && level == 3))
            {
                var ostanCode = userMarkaz?.CodeOstan;
                if (string.IsNullOrEmpty(ostanCode)) return result;

                var sums = await _context.MarkazReshtehs
                    .Where(mr =>
                        mr.Level == 4 &&
                        mr.TedadDaneshjoo > 0 &&
                        reshtehIds.Contains(mr.ReshtehId) &&
                        mr.Markaz != null &&
                        mr.Markaz.CodeOstan == ostanCode)
                    .GroupBy(mr => mr.ReshtehId)
                    .Select(g => new { ReshtehId = g.Key, Sum = g.Sum(mr => mr.TedadDaneshjoo ?? 0) })
                    .ToListAsync();

                foreach (var s in sums)
                    result[s.ReshtehId] = s.Sum;

                return result;
            }

            // نقش 2 یا نقش 1 + Level 2: جمع کل کشور
            if (codeRole == 2 || (codeRole == 1 && level == 2))
            {
                var sums = await _context.MarkazReshtehs
                    .Where(mr =>
                        mr.Level == 4 &&
                        mr.TedadDaneshjoo > 0 &&
                        reshtehIds.Contains(mr.ReshtehId))
                    .GroupBy(mr => mr.ReshtehId)
                    .Select(g => new { ReshtehId = g.Key, Sum = g.Sum(mr => mr.TedadDaneshjoo ?? 0) })
                    .ToListAsync();

                foreach (var s in sums)
                    result[s.ReshtehId] = s.Sum;

                return result;
            }

            return result;
        }

        // ============================================================
        // 8️⃣ افزودن لیستی
        // ============================================================
        [HttpPost("bulk-assign")]
        [Authorize]
        public async Task<IActionResult> BulkAssign([FromBody] MarkazReshtehBulkAssignDto dto)
        {
            try
            {
                var (user, role, userMarkaz, codeRole) = await _currentUser.GetCurrentUserInfoAsync();
                if (user == null || !codeRole.HasValue)
                    return Unauthorized(new { success = false, message = "اطلاعات کاربری نامعتبر است" });

                var allowedLevels = codeRole.Value switch
                {
                    1 => new[] { 2, 3, 4 },
                    2 => new[] { 2 },
                    3 => new[] { 3 },
                    4 => new[] { 4 },
                    _ => Array.Empty<int>()
                };

                if (!allowedLevels.Contains(dto.Level))
                    return Forbid();

                var markaz = await _context.Markazes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.MarkazId);

                if (markaz == null)
                    return BadRequest(new { success = false, message = "مرکز یافت نشد" });

                if (codeRole.Value == 4 && userMarkaz?.Id != dto.MarkazId)
                    return Forbid();

                if (codeRole.Value == 3 && markaz.CodeOstan != userMarkaz?.CodeOstan)
                    return Forbid();

                if (dto.Items == null || dto.Items.Count == 0)
                    return BadRequest(new { success = false, message = "لیست خالی است" });

                var reshtehIds = dto.Items.Select(i => i.ReshtehId).ToList();
                var calculatedTedads = await CalculateTedadsAsync(dto.Level, codeRole.Value, reshtehIds, userMarkaz);

                var rejected = new List<object>();
                var toInsert = new List<MarkazReshteh>();
                var processedKeys = new HashSet<int>();

                foreach (var item in dto.Items)
                {
                    if (!processedKeys.Add(item.ReshtehId))
                    {
                        rejected.Add(new { reshtehId = item.ReshtehId, reason = "در درخواست تکراری است" });
                        continue;
                    }

                    var reshtehExists = await _context.Reshtehs.AnyAsync(r => r.Id == item.ReshtehId);
                    if (!reshtehExists)
                    {
                        rejected.Add(new { reshtehId = item.ReshtehId, reason = "رشته یافت نشد" });
                        continue;
                    }

                    var existing = await _context.MarkazReshtehs
                        .AnyAsync(mr =>
                            mr.MarkazId == dto.MarkazId &&
                            mr.ReshtehId == item.ReshtehId &&
                            mr.Level == dto.Level);

                    if (existing)
                    {
                        rejected.Add(new { reshtehId = item.ReshtehId, reason = "قبلاً اضافه شده" });
                        continue;
                    }

                    int? finalTedad;
                    if (calculatedTedads.ContainsKey(item.ReshtehId))
                        finalTedad = calculatedTedads[item.ReshtehId];
                    else
                        finalTedad = item.TedadDaneshjoo;

                    toInsert.Add(new MarkazReshteh
                    {
                        MarkazId = dto.MarkazId,
                        ReshtehId = item.ReshtehId,
                        Level = dto.Level,
                        TedadDaneshjoo = finalTedad,
                        Vazeeat = item.Vazeeyat
                    });
                }

                int inserted = 0;
                if (toInsert.Any())
                {
                    await _context.MarkazReshtehs.AddRangeAsync(toInsert);
                    await _context.SaveChangesAsync();
                    inserted = toInsert.Count;
                }

                return Ok(new
                {
                    success = true,
                    message = $"{inserted} رکورد اضافه شد، {rejected.Count} رد شد.",
                    data = new { inserted, rejected = rejected.Count, total = dto.Items.Count },
                    rejectedItems = rejected
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در پردازش", error = ex.Message });
            }
        }
    }

}
