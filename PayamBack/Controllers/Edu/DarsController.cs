using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Dars;
using PayamBack.Models.Edu;
using PayamBack.Services.Edu;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class DarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string AllDarsCacheKey = "AllDarsList";
        private const int BulkBatchSize = 300;
        private readonly IDarsCacheService _darsCache;

        public DarsController(AppDbContext context,
            IMemoryCache cache,
            IDarsCacheService darsCache)
        {
            _context = context;
            _cache = cache;
            _darsCache = darsCache;
        }

        // ============================================================
        // 1️⃣ دریافت لیست درس‌ها (عمومی) - با فیلتر و صفحه‌بندی
        // ============================================================
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] int[]? reshtehIds = null,
            [FromQuery] int[]? grooheAmoozeshiIds = null,
            [FromQuery] string[]? maghtas = null,
            [FromQuery] string[]? daneshkades = null,
            [FromQuery] string[]? vahedTypes = null,
            [FromQuery] string[]? termAkhzs = null,
            [FromQuery] string[]? noeDarsList = null,
            [FromQuery] string? manbaSearch = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // 🔥 همه داده از کش (بدون Query به DB)
                var allData = await _darsCache.GetAllAsync();

                // 🔥 فیلتر در حافظه
                IEnumerable<DarsCacheDto> filtered = allData;

                if (!string.IsNullOrEmpty(search))
                {
                    var s = search.Trim();
                    filtered = filtered.Where(d =>
                        d.CodeDars.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        d.NaamDars.Contains(s, StringComparison.OrdinalIgnoreCase));
                }

                if (reshtehIds != null && reshtehIds.Length > 0)
                    filtered = filtered.Where(d => d.ReshtehId.HasValue && reshtehIds.Contains(d.ReshtehId.Value));

                if (grooheAmoozeshiIds != null && grooheAmoozeshiIds.Length > 0)
                    filtered = filtered.Where(d => d.GrooheAmoozeshiId.HasValue && grooheAmoozeshiIds.Contains(d.GrooheAmoozeshiId.Value));

                if (maghtas != null && maghtas.Length > 0)
                    filtered = filtered.Where(d => d.CodeMaghta != null && maghtas.Contains(d.CodeMaghta));

                if (daneshkades != null && daneshkades.Length > 0)
                    filtered = filtered.Where(d => d.CodeDaneshkadeh != null && daneshkades.Contains(d.CodeDaneshkadeh));

                if (vahedTypes != null && vahedTypes.Length > 0)
                {
                    filtered = filtered.Where(d =>
                        (vahedTypes.Contains("teori") && (d.VahedTeori ?? 0) > 0 && (d.VahedAmali ?? 0) == 0) ||
                        (vahedTypes.Contains("amali") && (d.VahedAmali ?? 0) > 0 && (d.VahedTeori ?? 0) == 0) ||
                        (vahedTypes.Contains("teori_amali") && (d.VahedTeori ?? 0) > 0 && (d.VahedAmali ?? 0) > 0));
                }

                if (termAkhzs != null && termAkhzs.Length > 0)
                {
                    var includeNull = termAkhzs.Contains("null");
                    var termValues = termAkhzs
                        .Where(t => t != "null")
                        .Select(t => int.TryParse(t, out var v) ? v : (int?)null)
                        .Where(v => v.HasValue)
                        .Select(v => v!.Value)
                        .ToList();

                    filtered = filtered.Where(d =>
                        (includeNull && !d.TermAkhz.HasValue) ||
                        (d.TermAkhz.HasValue && termValues.Contains(d.TermAkhz.Value)));
                }

                if (noeDarsList != null && noeDarsList.Length > 0)
                    filtered = filtered.Where(d => d.NoeDars != null && noeDarsList.Contains(d.NoeDars));

                if (!string.IsNullOrEmpty(manbaSearch))
                {
                    var m = manbaSearch.Trim();
                    filtered = filtered.Where(d =>
                        d.ManbaList.Any(mb =>
                            (mb.ShomareManba != null && mb.ShomareManba.Contains(m, StringComparison.OrdinalIgnoreCase)) ||
                            (mb.Onvan != null && mb.Onvan.Contains(m, StringComparison.OrdinalIgnoreCase))));
                }

                var list = filtered.ToList();
                var totalCount = list.Count;
                var pagedData = list
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "لیست درس‌ها دریافت شد",
                    data = pagedData,
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
                    message = "خطا در دریافت لیست درس‌ها",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dars = await _context.Dars
                    .AsNoTracking()
                    .Include(d => d.Reshteh)
                        .ThenInclude(r => r.GrooheAmoozeshi)
                    .Include(d => d.ManbaDarsList)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dars == null)
                    return NotFound(new { success = false, message = "درس یافت نشد" });

                var result = new DarsDetailDto
                {
                    Id = dars.Id,
                    CodeDars = dars.CodeDars,
                    NaamDars = dars.NaamDars,
                    VahedTeori = dars.VahedTeori,
                    VahedAmali = dars.VahedAmali,
                    SaatTeoriOrginal = dars.SaatTeoriOrginal,
                    SaatAmaliOrginal = dars.SaatAmaliOrginal,
                    SaatTeori = dars.SaatTeori,
                    SaatAmali = dars.SaatAmali,
                    TermAkhz = dars.TermAkhz,
                    NoeDars = dars.NoeDars,
                    NoeAzmoon = dars.NoeAzmoon,
                    ReshtehId = dars.ReshtehId,
                    ReshtehName = dars.Reshteh?.OnvanReshte,
                    GrooheAmoozeshiId = dars.Reshteh?.GrooheAmoozeshiId,
                    GrooheName = dars.Reshteh?.GrooheAmoozeshi?.OnvanGrooheAmoozeshi,
                    Zarfiat = dars.Zarfiat,
                    CreatedAt = dars.CreatedAt,
                    UpdatedAt = dars.UpdatedAt,
                    ManbaList = dars.ManbaDarsList?.Select(m => new ManbaDarsDetailDto
                    {
                        Id = m.Id,
                        DarsId = m.DarsId,
                        ShomareManba = m.ShomareManba,
                        NoeManba = m.NoeManba,
                        Onvan = m.Onvan,
                        Nevisandeh = m.Nevisandeh,
                        Motarjem = m.Motarjem,
                        SalEnteshar = m.SalEnteshar,
                        SalEntesharMiladi = m.SalEntesharMiladi,
                        Shabak = m.Shabak,
                        Nasher = m.Nasher,
                        NobateChap = m.NobateChap,
                        Vazeeyat = m.Vazeeyat,
                        CodePeyvast = m.CodePeyvast,
                        SharhPeyvast = m.SharhPeyvast,
                        TermUpdate = m.TermUpdate,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    }).ToList() ?? new List<ManbaDarsDetailDto>()
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات درس دریافت شد",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات درس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // نوع درس
        // ============================================================
        [HttpGet("noedars-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNoeDarsList()
        {
            try
            {
                const string cacheKey = "NoeDarsList";
                if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
                    return Ok(new { success = true, data = cached });

                var list = await _context.Dars
                    .Where(d => d.NoeDars != null && d.NoeDars != "")
                    .Select(d => d.NoeDars!)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToListAsync();

                _cache.Set(cacheKey, list, TimeSpan.FromHours(6));
                return Ok(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست نوع درس‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // منبع درس
        // ============================================================
        [HttpGet("manba-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetManbaList([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.ManbaDars.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    query = query.Where(m =>
                        (m.ShomareManba != null && m.ShomareManba.Contains(search)) ||
                        (m.Onvan != null && m.Onvan.Contains(search)));
                }

                var list = await query
                    .OrderBy(m => m.Id)
                    .Take(50)
                    .Select(m => new
                    {
                        m.Id,
                        m.ShomareManba,
                        m.Onvan,
                        m.CodePeyvast
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست منابع",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک درس با شناسه
        // ============================================================
        /*[HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dars = await _context.Dars
                    .Include(d => d.Reshteh)
                        .ThenInclude(r => r.GrooheAmoozeshi)
                    .Include(d => d.ManbaDarsList)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dars == null)
                    return NotFound(new { success = false, message = "درس یافت نشد" });

                var result = new DarsDetailDto
                {
                    Id = dars.Id,
                    CodeDars = dars.CodeDars,
                    NaamDars = dars.NaamDars,
                    VahedTeori = dars.VahedTeori,
                    VahedAmali = dars.VahedAmali,
                    SaatTeoriOrginal = dars.SaatTeoriOrginal,
                    SaatAmaliOrginal = dars.SaatAmaliOrginal,
                    SaatTeori = dars.SaatTeori,
                    SaatAmali = dars.SaatAmali,
                    TermAkhz = dars.TermAkhz,
                    NoeDars = dars.NoeDars,
                    NoeAzmoon = dars.NoeAzmoon,
                    ReshtehId = dars.ReshtehId,
                    ReshtehName = dars.Reshteh?.OnvanReshte,
                    GrooheAmoozeshiId = dars.Reshteh?.GrooheAmoozeshiId,
                    GrooheName = dars.Reshteh?.GrooheAmoozeshi?.OnvanGrooheAmoozeshi,
                    Zarfiat = dars.Zarfiat,
                    CreatedAt = dars.CreatedAt,
                    UpdatedAt = dars.UpdatedAt,
                    ManbaList = dars.ManbaDarsList?.Select(m => new ManbaDarsDetailDto
                    {
                        Id = m.Id,
                        DarsId = m.DarsId,
                        ShomareManba = m.ShomareManba,
                        NoeManba = m.NoeManba,
                        Onvan = m.Onvan,
                        Nevisandeh = m.Nevisandeh,
                        Motarjem = m.Motarjem,
                        SalEnteshar = m.SalEnteshar,
                        SalEntesharMiladi = m.SalEntesharMiladi,
                        Shabak = m.Shabak,
                        Nasher = m.Nasher,
                        NobateChap = m.NobateChap,
                        Vazeeyat = m.Vazeeyat,
                        CodePeyvast = m.CodePeyvast,
                        SharhPeyvast = m.SharhPeyvast,
                        TermUpdate = m.TermUpdate,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    }).ToList() ?? new List<ManbaDarsDetailDto>()
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات درس دریافت شد",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات درس",
                    error = ex.Message
                });
            }
        }*/

        // ============================================================
        // 3️⃣ ایجاد درس جدید (تکی)
        // ============================================================
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] DarsCreateDto dto)
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

                if (dto.ReshtehId.HasValue)
                {
                    var reshtehExists = await _context.Reshtehs
                        .AnyAsync(r => r.Id == dto.ReshtehId.Value);

                    if (!reshtehExists)
                        return BadRequest(new { success = false, message = "رشته یافت نشد" });
                }

                var exists = await _context.Dars
                    .AnyAsync(d =>
                        d.CodeDars == dto.CodeDars &&
                        d.ReshtehId == dto.ReshtehId);

                if (exists)
                    return BadRequest(new
                    {
                        success = false,
                        message = "درسی با این کد در این رشته قبلاً ثبت شده است"
                    });

                var currentTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => t.CodeTerm)
                    .FirstOrDefaultAsync();

                var dars = new Dars
                {
                    CodeDars = dto.CodeDars,
                    NaamDars = dto.NaamDars,
                    VahedTeori = dto.VahedTeori,
                    VahedAmali = dto.VahedAmali,
                    SaatTeoriOrginal = dto.SaatTeoriOrginal,
                    SaatAmaliOrginal = dto.SaatAmaliOrginal,
                    SaatTeori = dto.SaatTeori,
                    SaatAmali = dto.SaatAmali,
                    TermAkhz = dto.TermAkhz,
                    NoeDars = dto.NoeDars,
                    NoeAzmoon = dto.NoeAzmoon,
                    ReshtehId = dto.ReshtehId,
                    Zarfiat = dto.Zarfiat,
                    CreatedAt = DateTime.Now,
                    ManbaDarsList = dto.ManbaList?.Select(m => new ManbaDars
                    {
                        ShomareManba = m.ShomareManba,
                        NoeManba = m.NoeManba,
                        Onvan = m.Onvan ?? string.Empty,
                        Nevisandeh = m.Nevisandeh,
                        Motarjem = m.Motarjem,
                        SalEnteshar = m.SalEnteshar,
                        SalEntesharMiladi = m.SalEntesharMiladi,
                        Shabak = m.Shabak,
                        Nasher = m.Nasher,
                        NobateChap = m.NobateChap,
                        Vazeeyat = m.Vazeeyat,
                        CodePeyvast = m.CodePeyvast,
                        SharhPeyvast = m.SharhPeyvast,
                        TermUpdate = currentTerm,
                        CreatedAt = DateTime.Now
                    }).ToList() ?? new List<ManbaDars>()
                };

                await _context.Dars.AddAsync(dars);
                await _context.SaveChangesAsync();

                await _darsCache.RebuildAsync();

                return Ok(new
                {
                    success = true,
                    message = "درس با موفقیت ایجاد شد",
                    data = new { id = dars.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد درس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش درس (تکی)
        // ============================================================
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] DarsUpdateDto dto)
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

                var dars = await _context.Dars
                    .Include(d => d.ManbaDarsList)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dars == null)
                    return NotFound(new { success = false, message = "درس یافت نشد" });

                if (dto.ReshtehId.HasValue)
                {
                    var reshtehExists = await _context.Reshtehs
                        .AnyAsync(r => r.Id == dto.ReshtehId.Value);

                    if (!reshtehExists)
                        return BadRequest(new { success = false, message = "رشته یافت نشد" });
                }

                var targetCodeDars = !string.IsNullOrEmpty(dto.CodeDars) ? dto.CodeDars : dars.CodeDars;
                var targetReshtehId = dto.ReshtehId ?? dars.ReshtehId;

                if (targetCodeDars != dars.CodeDars || targetReshtehId != dars.ReshtehId)
                {
                    var exists = await _context.Dars
                        .AnyAsync(d =>
                            d.Id != id &&
                            d.CodeDars == targetCodeDars &&
                            d.ReshtehId == targetReshtehId);

                    if (exists)
                        return BadRequest(new
                        {
                            success = false,
                            message = "درسی با این کد در این رشته قبلاً ثبت شده است"
                        });
                }

                dars.CodeDars = dto.CodeDars ?? dars.CodeDars;
                dars.NaamDars = dto.NaamDars ?? dars.NaamDars;
                dars.VahedTeori = dto.VahedTeori ?? dars.VahedTeori;
                dars.VahedAmali = dto.VahedAmali ?? dars.VahedAmali;
                dars.SaatTeoriOrginal = dto.SaatTeoriOrginal ?? dars.SaatTeoriOrginal;
                dars.SaatAmaliOrginal = dto.SaatAmaliOrginal ?? dars.SaatAmaliOrginal;
                dars.SaatTeori = dto.SaatTeori ?? dars.SaatTeori;
                dars.SaatAmali = dto.SaatAmali ?? dars.SaatAmali;
                dars.TermAkhz = dto.TermAkhz ?? dars.TermAkhz;
                dars.NoeDars = dto.NoeDars ?? dars.NoeDars;
                dars.NoeAzmoon = dto.NoeAzmoon ?? dars.NoeAzmoon;
                dars.ReshtehId = dto.ReshtehId ?? dars.ReshtehId;
                dars.Zarfiat = dto.Zarfiat ?? dars.Zarfiat;
                dars.UpdatedAt = DateTime.Now;

                var existingManbas = dars.ManbaDarsList?.ToList() ?? new List<ManbaDars>();

                var currentTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => t.CodeTerm)
                    .FirstOrDefaultAsync();

                var incomingIds = dto.ManbaList?
                    .Where(m => m.Id > 0)
                    .Select(m => m.Id)
                    .ToHashSet() ?? new HashSet<int>();

                var manbasToDelete = existingManbas
                    .Where(m => !incomingIds.Contains(m.Id))
                    .ToList();

                if (manbasToDelete.Any())
                    _context.ManbaDars.RemoveRange(manbasToDelete);

                if (dto.ManbaList != null && dto.ManbaList.Any())
                {
                    foreach (var manbaDto in dto.ManbaList)
                    {
                        if (manbaDto.Id > 0)
                        {
                            var existingManba = existingManbas.FirstOrDefault(m => m.Id == manbaDto.Id);
                            if (existingManba != null)
                            {
                                bool hasChanged =
                                    existingManba.ShomareManba != manbaDto.ShomareManba ||
                                    existingManba.NoeManba != manbaDto.NoeManba ||
                                    existingManba.Onvan != (manbaDto.Onvan ?? string.Empty) ||
                                    existingManba.Nevisandeh != manbaDto.Nevisandeh ||
                                    existingManba.Motarjem != manbaDto.Motarjem ||
                                    existingManba.SalEnteshar != manbaDto.SalEnteshar ||
                                    existingManba.SalEntesharMiladi != manbaDto.SalEntesharMiladi ||
                                    existingManba.Shabak != manbaDto.Shabak ||
                                    existingManba.Nasher != manbaDto.Nasher ||
                                    existingManba.NobateChap != manbaDto.NobateChap ||
                                    existingManba.Vazeeyat != manbaDto.Vazeeyat ||
                                    existingManba.CodePeyvast != manbaDto.CodePeyvast ||
                                    existingManba.SharhPeyvast != manbaDto.SharhPeyvast;

                                if (hasChanged)
                                {
                                    existingManba.ShomareManba = manbaDto.ShomareManba;
                                    existingManba.NoeManba = manbaDto.NoeManba;
                                    existingManba.Onvan = manbaDto.Onvan ?? string.Empty;
                                    existingManba.Nevisandeh = manbaDto.Nevisandeh;
                                    existingManba.Motarjem = manbaDto.Motarjem;
                                    existingManba.SalEnteshar = manbaDto.SalEnteshar;
                                    existingManba.SalEntesharMiladi = manbaDto.SalEntesharMiladi;
                                    existingManba.Shabak = manbaDto.Shabak;
                                    existingManba.Nasher = manbaDto.Nasher;
                                    existingManba.NobateChap = manbaDto.NobateChap;
                                    existingManba.Vazeeyat = manbaDto.Vazeeyat;
                                    existingManba.CodePeyvast = manbaDto.CodePeyvast;
                                    existingManba.SharhPeyvast = manbaDto.SharhPeyvast;
                                    existingManba.TermUpdate = currentTerm;
                                    existingManba.UpdatedAt = DateTime.Now;
                                }
                            }
                        }
                        else
                        {
                            var newManba = new ManbaDars
                            {
                                DarsId = dars.Id,
                                ShomareManba = manbaDto.ShomareManba,
                                NoeManba = manbaDto.NoeManba,
                                Onvan = manbaDto.Onvan ?? string.Empty,
                                Nevisandeh = manbaDto.Nevisandeh,
                                Motarjem = manbaDto.Motarjem,
                                SalEnteshar = manbaDto.SalEnteshar,
                                SalEntesharMiladi = manbaDto.SalEntesharMiladi,
                                Shabak = manbaDto.Shabak,
                                Nasher = manbaDto.Nasher,
                                NobateChap = manbaDto.NobateChap,
                                Vazeeyat = manbaDto.Vazeeyat,
                                CodePeyvast = manbaDto.CodePeyvast,
                                SharhPeyvast = manbaDto.SharhPeyvast,
                                TermUpdate = currentTerm,
                                CreatedAt = DateTime.Now
                            };
                            await _context.ManbaDars.AddAsync(newManba);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _darsCache.RebuildAsync();

                return Ok(new
                {
                    success = true,
                    message = "درس با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش درس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف درس
        // ============================================================
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dars = await _context.Dars.FirstOrDefaultAsync(d => d.Id == id);

                if (dars == null)
                    return NotFound(new { success = false, message = "درس یافت نشد" });

                var hasManba = await _context.ManbaDars.AnyAsync(m => m.DarsId == id);
                if (hasManba)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این درس دارای منبع است و قابل حذف نیست. ابتدا منابع را حذف کنید"
                    });

                _context.Dars.Remove(dars);
                await _context.SaveChangesAsync();
                await _darsCache.RebuildAsync();

                return Ok(new { success = true, message = "درس با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف درس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ پاک کردن کش
        // ============================================================
        [HttpDelete("clear-cache")]
        [Authorize]
        public async Task<IActionResult> ClearCache()
        {
            await _darsCache.RebuildAsync();
            return Ok(new { success = true, message = "کش درس‌ها بازسازی شد" });
        }

        // ============================================================
        // 7️⃣ آپلود گروهی درس‌ها و منابع از فایل اکسل
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
                // ۱. خواندن فایل اکسل
                // ============================================================
                List<DarsImportRow> rows;
                try
                {
                    using var stream = file.OpenReadStream();
                    rows = ParseExcelFile(stream);
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
                // ۲. گروه‌بندی به درس+منابع (هر درس یک آیتم، منابعهاش همراهش)
                // ============================================================
                var allDarsWithManba = GroupIntoDarsWithManba(rows);

                // ============================================================
                // ۳. بارگذاری رشته‌ها و درس‌ها از دیتابیس (یک بار)
                // ============================================================
                var allReshtehs = await _context.Reshtehs
                    .AsNoTracking()
                    .ToListAsync();

                // کلید یکتای رشته: (CodeReshte, CodeMaghta, TermVorood, TermEamal)
                var reshtehLookup = allReshtehs
                    .GroupBy(r => new ReshtehKey(
                        r.CodeReshte ?? "",
                        r.CodeMaghta ?? "",
                        r.TermVorood ?? "",
                        r.TermEamal ?? ""))
                    .ToDictionary(g => g.Key, g => g.First());

                // ترم جاری
                var currentTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => t.CodeTerm)
                    .FirstOrDefaultAsync();

                // ============================================================
                // ۴. آماده‌سازی نتیجه
                // ============================================================
                var result = new BulkUploadResultDto();

                // ============================================================
                // ۵. پردازش دسته‌ای — هر ۳۰۰ درس
                // ============================================================
                int batchCounter = 0;

                foreach (var item in allDarsWithManba)
                {
                    try
                    {
                        // ---- 5.1) پیدا کردن رشته ----
                        var reshtehKey = new ReshtehKey(
                            item.CodeReshte ?? "",
                            item.CodeMaghta ?? "",
                            item.TermVorood ?? "",
                            item.TermEamal ?? "");

                        if (!reshtehLookup.TryGetValue(reshtehKey, out var reshteh))
                        {
                            // رشته پیدا نشد → همه‌ی درس‌های این رشته (از این آیتم به بعد تا رشته عوض شه؟)
                            // طبق سوال ۸: هر درس این رشته رو توی خطا بیار
                            result.Errors.Add(new BulkUploadErrorDto
                            {
                                RowNumber = item.FirstRowNumber,
                                CodeReshte = item.CodeReshte,
                                CodeMaghta = item.CodeMaghta,
                                TermVorood = item.TermVorood,
                                TermEamal = item.TermEamal,
                                CodeDars = item.CodeDars,
                                Stage = "Reshteh",
                                ErrorMessage = $"رشته یافت نشد. کلید: ({item.CodeReshte}, {item.CodeMaghta}, {item.TermVorood}, {item.TermEamal})"
                            });
                            result.ErrorCount++;
                            continue;
                        }

                        // ---- 5.2) پیدا کردن درس (توی DB) ----
                        var existingDars = await _context.Dars
                            .Include(d => d.ManbaDarsList)
                            .FirstOrDefaultAsync(d =>
                                d.ReshtehId == reshteh.Id &&
                                d.CodeDars == item.CodeDars);

                        if (existingDars != null)
                        {
                            // ==================== UPDATE ====================
                            UpdateDarsFromImport(existingDars, item);
                            SyncManbas(existingDars, item.ManbaList, currentTerm, item, result);
                            result.UpdatedDars++;
                        }
                        else
                        {
                            // ==================== INSERT ====================
                            var newDars = CreateDarsFromImport(item, reshteh.Id);
                            _context.Dars.Add(newDars);
                            await _context.SaveChangesAsync(); // برای گرفتن Id درس جدید

                            SyncManbas(newDars, item.ManbaList, currentTerm, item, result);
                            result.InsertedDars++;
                        }

                        await _context.SaveChangesAsync();
                        batchCounter++;

                        // ---- 5.3) اگر دسته پر شد، ChangeTracker رو خالی کن ----
                        if (batchCounter >= BulkBatchSize)
                        {
                            _context.ChangeTracker.Clear();
                            batchCounter = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        // خطا در یک درس → فقط همون skip، بقیه ادامه
                        result.Errors.Add(new BulkUploadErrorDto
                        {
                            RowNumber = item.FirstRowNumber,
                            CodeReshte = item.CodeReshte,
                            CodeMaghta = item.CodeMaghta,
                            TermVorood = item.TermVorood,
                            TermEamal = item.TermEamal,
                            CodeDars = item.CodeDars,
                            Stage = "Dars",
                            ErrorMessage = ex.Message
                        });
                        result.ErrorCount++;

                        // اگه خطا باعث dirty شدن ChangeTracker شد، پاکش کن
                        // ولی درس‌های قبلی که SaveChanges شدن، در DB باقی می‌مونن
                        try { _context.ChangeTracker.Clear(); } catch { }
                        batchCounter = 0;
                    }
                }

                // ============================================================
                // ۶. پاک کردن کش — در هر شرایطی
                // ============================================================
                await _darsCache.RebuildAsync();

                // ============================================================
                // ۷. ساخت فایل خطا (فقط اگه خطا بود)
                // ============================================================
                string? errorFileBase64 = null;
                if (result.ErrorCount > 0)
                {
                    errorFileBase64 = GenerateErrorExcel(result);
                }

                return Ok(new
                {
                    success = true,
                    message = $"عملیات انجام شد. " +
                              $"{result.InsertedDars} درس جدید، " +
                              $"{result.UpdatedDars} درس آپدیت، " +
                              $"{result.InsertedManba} منبع جدید، " +
                              $"{result.UpdatedManba} منبع آپدیت، " +
                              $"{result.DeletedManba} منبع حذف، " +
                              $"{result.ErrorCount} خطا.",
                    data = new
                    {
                        insertedDars = result.InsertedDars,
                        updatedDars = result.UpdatedDars,
                        insertedManba = result.InsertedManba,
                        updatedManba = result.UpdatedManba,
                        deletedManba = result.DeletedManba,
                        errorCount = result.ErrorCount
                    },
                    errors = result.Errors,
                    errorFile = errorFileBase64,
                    errorFileName = errorFileBase64 != null ? "bulk-upload-errors.xlsx" : null
                });
            }
            catch (Exception ex)
            {
                // خطای کلی (مثلاً فایل خرابه) — ولی کش رو در هر شرایطی پاک کن
                try { await _darsCache.RebuildAsync(); } catch { }

                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در پردازش فایل",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // متدهای کمکی BulkUpload
        // ============================================================

        /// <summary>
        /// خواندن فایل اکسل و تبدیل به لیست ردیف‌های خام.
        /// </summary>
        private List<DarsImportRow> ParseExcelFile(Stream stream)
        {
            var rows = new List<DarsImportRow>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var importRow = new DarsImportRow
                {
                    RowNumber = row,
                    CodeDars = GetStringValue(worksheet, row, 4),
                    NaamDars = GetStringValue(worksheet, row, 5),
                    VahedTeori = GetDecimalValue(worksheet, row, 6),
                    VahedAmali = GetDecimalValue(worksheet, row, 7),

                    // طبق توافق: یک سلول مشترک برای هر جفت
                    SaatTeori_File = GetIntValue(worksheet, row, 8),
                    SaatAmali_File = GetIntValue(worksheet, row, 9),

                    TermAkhz = GetIntValue(worksheet, row, 10),
                    NoeDars = GetStringValue(worksheet, row, 11),
                    NoeAzmoon = GetStringValue(worksheet, row, 12),
                    Zarfiat = GetIntValue(worksheet, row, 28),
                    TermUpdate = GetStringValue(worksheet, row, 3),

                    // رشته
                    CodeReshte = GetStringValue(worksheet, row, 26),
                    CodeMaghta = GetStringValue(worksheet, row, 27),
                    TermVorood = GetStringValue(worksheet, row, 37),
                    TermEamal = GetStringValue(worksheet, row, 38),
                    ReshtehName = GetStringValue(worksheet, row, 34),

                    // منبع
                    ShomareManba = GetStringValue(worksheet, row, 13),
                    NoeManba = GetStringValue(worksheet, row, 14),
                    OnvanManba = GetStringValue(worksheet, row, 15),
                    Nevisandeh = GetStringValue(worksheet, row, 16),
                    Motarjem = GetStringValue(worksheet, row, 17),
                    SalEnteshar = GetStringValue(worksheet, row, 18),
                    SalEntesharMiladi = GetStringValue(worksheet, row, 19),
                    Shabak = GetStringValue(worksheet, row, 20),
                    Nasher = GetStringValue(worksheet, row, 21),
                    NobateChap = GetStringValue(worksheet, row, 29),
                    Vazeeyat = GetStringValue(worksheet, row, 22),
                    CodePeyvast = GetStringValue(worksheet, row, 23),
                    SharhPeyvast = GetStringValue(worksheet, row, 24)
                };

                rows.Add(importRow);
            }

            return rows;
        }

        private string? GetStringValue(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            var value = cell.GetString()?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private int? GetIntValue(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<int>(out var value)) return value;
            return null;
        }

        private decimal? GetDecimalValue(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<decimal>(out var value)) return value;
            return null;
        }

        /// <summary>
        /// گروه‌بندی ردیف‌های خام به لیست درس‌های دارای منابع.
        /// هر ردیف با CodeDars پر = شروع درس جدید
        /// هر ردیف با CodeDars خالی = منبع اضافی برای درس قبلی
        /// </summary>
        private List<DarsWithManba> GroupIntoDarsWithManba(List<DarsImportRow> rows)
        {
            var result = new List<DarsWithManba>();
            DarsWithManba? currentDars = null;

            foreach (var row in rows)
            {
                // اگه کد درس خالیه و هیچ درس قبلی‌ای نیست، این ردیف بی‌ربطه
                if (string.IsNullOrWhiteSpace(row.CodeDars) && currentDars == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(row.CodeDars))
                {
                    // شروع درس جدید
                    currentDars = new DarsWithManba
                    {
                        FirstRowNumber = row.RowNumber,
                        CodeReshte = row.CodeReshte,
                        CodeMaghta = row.CodeMaghta,
                        TermVorood = row.TermVorood,
                        TermEamal = row.TermEamal,
                        ReshtehName = row.ReshtehName,

                        CodeDars = row.CodeDars!,
                        NaamDars = row.NaamDars,
                        VahedTeori = row.VahedTeori,
                        VahedAmali = row.VahedAmali,
                        SaatTeori_File = row.SaatTeori_File,
                        SaatAmali_File = row.SaatAmali_File,
                        TermAkhz = row.TermAkhz,
                        NoeDars = row.NoeDars,
                        NoeAzmoon = row.NoeAzmoon,
                        Zarfiat = row.Zarfiat,

                        ManbaList = new List<ManbaImportRow>()
                    };

                    // منبع اول توی همون ردیف درس
                    if (!string.IsNullOrEmpty(row.OnvanManba) || !string.IsNullOrEmpty(row.ShomareManba))
                        currentDars.ManbaList.Add(CreateManbaFromRow(row));

                    result.Add(currentDars);
                }
                else if (currentDars != null)
                {
                    // منبع اضافی برای درس قبلی
                    if (!string.IsNullOrEmpty(row.OnvanManba) || !string.IsNullOrEmpty(row.ShomareManba))
                        currentDars.ManbaList.Add(CreateManbaFromRow(row));
                }
            }

            return result;
        }

        private ManbaImportRow CreateManbaFromRow(DarsImportRow row)
        {
            return new ManbaImportRow
            {
                ShomareManba = row.ShomareManba,
                NoeManba = row.NoeManba,
                Onvan = row.OnvanManba,
                Nevisandeh = row.Nevisandeh,
                Motarjem = row.Motarjem,
                SalEnteshar = row.SalEnteshar,
                SalEntesharMiladi = row.SalEntesharMiladi,
                Shabak = row.Shabak,
                Nasher = row.Nasher,
                NobateChap = row.NobateChap,
                Vazeeyat = row.Vazeeyat,
                CodePeyvast = row.CodePeyvast,
                SharhPeyvast = row.SharhPeyvast
            };
        }

        /// <summary>
        /// ساخت درس جدید از آیتم ایمپورت.
        /// </summary>
        private Dars CreateDarsFromImport(DarsWithManba item, int reshtehId)
        {
            var saatTeori = item.SaatTeori_File ?? 0;
            var saatAmali = item.SaatAmali_File ?? 0;

            return new Dars
            {
                CodeDars = item.CodeDars,
                NaamDars = item.NaamDars,
                VahedTeori = item.VahedTeori,
                VahedAmali = item.VahedAmali,

                // برای درس جدید، هر دو از فایل (چون یکی هستن)
                SaatTeori = saatTeori,
                SaatTeoriOrginal = saatTeori,
                SaatAmali = saatAmali,
                SaatAmaliOrginal = saatAmali,

                TermAkhz = item.TermAkhz,
                NoeDars = item.NoeDars,
                NoeAzmoon = item.NoeAzmoon,
                ReshtehId = reshtehId,
                Zarfiat = item.Zarfiat,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ManbaDarsList = new List<ManbaDars>()
            };
        }

        /// <summary>
        /// آپدیت درس موجود از آیتم ایمپورت.
        /// </summary>
        private void UpdateDarsFromImport(Dars dars, DarsWithManba item)
        {
            // فیلدهای ساده — همیشه از فایل
            dars.NaamDars = item.NaamDars ?? dars.NaamDars;
            dars.VahedTeori = item.VahedTeori ?? dars.VahedTeori;
            dars.VahedAmali = item.VahedAmali ?? dars.VahedAmali;
            dars.TermAkhz = item.TermAkhz ?? dars.TermAkhz;
            dars.NoeDars = item.NoeDars ?? dars.NoeDars;
            dars.NoeAzmoon = item.NoeAzmoon ?? dars.NoeAzmoon;
            dars.Zarfiat = item.Zarfiat ?? dars.Zarfiat;

            // منطق شرطی ساعت تئوری
            var fileSaatTeori = item.SaatTeori_File ?? 0;

            if (dars.SaatTeori == dars.SaatTeoriOrginal)
            {
                // کاربر دست نزده → هر دو از فایل
                dars.SaatTeori = fileSaatTeori;
                dars.SaatTeoriOrginal = fileSaatTeori;
            }
            else
            {
                // کاربر دست زده → فقط Orginal از فایل
                dars.SaatTeoriOrginal = fileSaatTeori;
            }

            // منطق شرطی ساعت عملی
            var fileSaatAmali = item.SaatAmali_File ?? 0;

            if (dars.SaatAmali == dars.SaatAmaliOrginal)
            {
                dars.SaatAmali = fileSaatAmali;
                dars.SaatAmaliOrginal = fileSaatAmali;
            }
            else
            {
                dars.SaatAmaliOrginal = fileSaatAmali;
            }

            dars.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Sync منابع یک درس بر اساس کلید یکتا (DarsId, ShomareManba, CodePeyvast).
        /// - موجود در فایل و DB → Update
        /// - موجود در فایل، غایب در DB → Insert
        /// - غایب در فایل، موجود در DB → Delete
        /// </summary>
        private void SyncManbas(
            Dars dars,
            List<ManbaImportRow> incomingManbas,
            string? currentTerm,
            DarsWithManba item,
            BulkUploadResultDto result)
        {
            dars.ManbaDarsList ??= new List<ManbaDars>();

            static string MakeKey(string? shomare, string? codePeyvast)
                => $"{(shomare ?? "").Trim()}|{(codePeyvast ?? "").Trim()}";

            // فقط منابع معتبر (حداقل یه ShomareManba یا Onvan داشته باشن)
            var validIncoming = incomingManbas
                .Where(m => !string.IsNullOrWhiteSpace(m.ShomareManba)
                         || !string.IsNullOrWhiteSpace(m.Onvan))
                .ToList();

            var incomingKeys = validIncoming
                .Select(m => MakeKey(m.ShomareManba, m.CodePeyvast))
                .ToHashSet();

            // ---- 1) حذف منابعی که توی فایل نیستن ----
            var toDelete = dars.ManbaDarsList
                .Where(m => !incomingKeys.Contains(MakeKey(m.ShomareManba, m.CodePeyvast)))
                .ToList();

            foreach (var manba in toDelete)
            {
                _context.ManbaDars.Remove(manba);
                result.DeletedManba++;
            }

            // ---- 2) Insert / Update ----
            var existingByKey = dars.ManbaDarsList
                .Where(m => !toDelete.Contains(m))
                .GroupBy(m => MakeKey(m.ShomareManba, m.CodePeyvast))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var row in validIncoming)
            {
                try
                {
                    var key = MakeKey(row.ShomareManba, row.CodePeyvast);

                    if (existingByKey.TryGetValue(key, out var existing))
                    {
                        // --------- UPDATE ---------
                        ApplyManbaUpdate(existing, row, currentTerm);
                        result.UpdatedManba++;
                    }
                    else
                    {
                        // --------- INSERT ---------
                        var newManba = CreateNewManba(row, dars.Id, currentTerm);
                        dars.ManbaDarsList.Add(newManba);
                        result.InsertedManba++;
                    }
                }
                catch (Exception ex)
                {
                    // خطای یک منبع، بقیه رو متوقف نکن
                    result.Errors.Add(new BulkUploadErrorDto
                    {
                        RowNumber = item.FirstRowNumber,
                        CodeReshte = item.CodeReshte,
                        CodeMaghta = item.CodeMaghta,
                        TermVorood = item.TermVorood,
                        TermEamal = item.TermEamal,
                        CodeDars = item.CodeDars,
                        ShomareManba = row.ShomareManba,
                        Stage = "Manba",
                        ErrorMessage = ex.Message
                    });
                    result.ErrorCount++;
                }
            }
        }

        private void ApplyManbaUpdate(ManbaDars manba, ManbaImportRow row, string? currentTerm)
        {
            bool hasChanged =
                manba.Onvan != (row.Onvan ?? string.Empty) ||
                manba.NoeManba != row.NoeManba ||
                manba.Nevisandeh != row.Nevisandeh ||
                manba.Motarjem != row.Motarjem ||
                manba.SalEnteshar != row.SalEnteshar ||
                manba.SalEntesharMiladi != row.SalEntesharMiladi ||
                manba.Shabak != row.Shabak ||
                manba.Nasher != row.Nasher ||
                manba.NobateChap != row.NobateChap ||
                manba.Vazeeyat != row.Vazeeyat ||
                manba.SharhPeyvast != row.SharhPeyvast;

            manba.Onvan = row.Onvan ?? string.Empty;
            manba.NoeManba = row.NoeManba;
            manba.Nevisandeh = row.Nevisandeh;
            manba.Motarjem = row.Motarjem;
            manba.SalEnteshar = row.SalEnteshar;
            manba.SalEntesharMiladi = row.SalEntesharMiladi;
            manba.Shabak = row.Shabak;
            manba.Nasher = row.Nasher;
            manba.NobateChap = row.NobateChap;
            manba.Vazeeyat = row.Vazeeyat;
            manba.SharhPeyvast = row.SharhPeyvast;

            if (hasChanged)
            {
                manba.TermUpdate = currentTerm;
                manba.UpdatedAt = DateTime.Now;
            }
        }

        private ManbaDars CreateNewManba(ManbaImportRow row, int darsId, string? currentTerm)
        {
            return new ManbaDars
            {
                DarsId = darsId,
                ShomareManba = row.ShomareManba,
                NoeManba = row.NoeManba,
                Onvan = row.Onvan ?? string.Empty,
                Nevisandeh = row.Nevisandeh,
                Motarjem = row.Motarjem,
                SalEnteshar = row.SalEnteshar,
                SalEntesharMiladi = row.SalEntesharMiladi,
                Shabak = row.Shabak,
                Nasher = row.Nasher,
                NobateChap = row.NobateChap,
                Vazeeyat = row.Vazeeyat,
                CodePeyvast = row.CodePeyvast,
                SharhPeyvast = row.SharhPeyvast,
                TermUpdate = currentTerm,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// ساخت فایل اکسل خطاها.
        /// </summary>
        private string GenerateErrorExcel(BulkUploadResultDto result)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("خطاها");

            worksheet.Cell(1, 1).Value = "ردیف";
            worksheet.Cell(1, 2).Value = "شماره ردیف اکسل";
            worksheet.Cell(1, 3).Value = "کد رشته";
            worksheet.Cell(1, 4).Value = "کد مقطع";
            worksheet.Cell(1, 5).Value = "ترم ورود";
            worksheet.Cell(1, 6).Value = "ترم اعمال";
            worksheet.Cell(1, 7).Value = "کد درس";
            worksheet.Cell(1, 8).Value = "شماره منبع";
            worksheet.Cell(1, 9).Value = "مرحله";
            worksheet.Cell(1, 10).Value = "پیام خطا";

            var headerRange = worksheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int rowNum = 2;
            foreach (var item in result.Errors)
            {
                worksheet.Cell(rowNum, 1).Value = rowNum - 1;
                worksheet.Cell(rowNum, 2).Value = item.RowNumber?.ToString() ?? "-";
                worksheet.Cell(rowNum, 3).Value = item.CodeReshte ?? "-";
                worksheet.Cell(rowNum, 4).Value = item.CodeMaghta ?? "-";
                worksheet.Cell(rowNum, 5).Value = item.TermVorood ?? "-";
                worksheet.Cell(rowNum, 6).Value = item.TermEamal ?? "-";
                worksheet.Cell(rowNum, 7).Value = item.CodeDars ?? "-";
                worksheet.Cell(rowNum, 8).Value = item.ShomareManba ?? "-";
                worksheet.Cell(rowNum, 9).Value = item.Stage ?? "-";
                worksheet.Cell(rowNum, 10).Value = item.ErrorMessage ?? "-";
                rowNum++;
            }

            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 12;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 15;
            worksheet.Column(9).Width = 12;
            worksheet.Column(10).Width = 60;

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        // ============================================================
        // کلاس‌های کمکی داخلی (Private Nested)
        // ============================================================

        /// <summary>
        /// کلید یکتای رشته.
        /// </summary>
        private record ReshtehKey(string CodeReshte, string CodeMaghta, string TermVorood, string TermEamal);

        private class DarsImportRow
        {
            public int RowNumber { get; set; }
            public string? CodeDars { get; set; }
            public string? NaamDars { get; set; }
            public decimal? VahedTeori { get; set; }
            public decimal? VahedAmali { get; set; }
            public int? SaatTeori_File { get; set; }
            public int? SaatAmali_File { get; set; }
            public int? TermAkhz { get; set; }
            public string? NoeDars { get; set; }
            public string? NoeAzmoon { get; set; }
            public int? Zarfiat { get; set; }
            public string? TermUpdate { get; set; }

            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? ReshtehName { get; set; }

            public string? ShomareManba { get; set; }
            public string? NoeManba { get; set; }
            public string? OnvanManba { get; set; }
            public string? Nevisandeh { get; set; }
            public string? Motarjem { get; set; }
            public string? SalEnteshar { get; set; }
            public string? SalEntesharMiladi { get; set; }
            public string? Shabak { get; set; }
            public string? Nasher { get; set; }
            public string? NobateChap { get; set; }
            public string? Vazeeyat { get; set; }
            public string? CodePeyvast { get; set; }
            public string? SharhPeyvast { get; set; }
        }

        private class DarsWithManba
        {
            public int FirstRowNumber { get; set; }

            // اطلاعات رشته
            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? ReshtehName { get; set; }

            // اطلاعات درس
            public string CodeDars { get; set; } = string.Empty;
            public string? NaamDars { get; set; }
            public decimal? VahedTeori { get; set; }
            public decimal? VahedAmali { get; set; }
            public int? SaatTeori_File { get; set; }
            public int? SaatAmali_File { get; set; }
            public int? TermAkhz { get; set; }
            public string? NoeDars { get; set; }
            public string? NoeAzmoon { get; set; }
            public int? Zarfiat { get; set; }

            // منابع
            public List<ManbaImportRow> ManbaList { get; set; } = new();
        }

        private class ManbaImportRow
        {
            public string? ShomareManba { get; set; }
            public string? NoeManba { get; set; }
            public string? Onvan { get; set; }
            public string? Nevisandeh { get; set; }
            public string? Motarjem { get; set; }
            public string? SalEnteshar { get; set; }
            public string? SalEntesharMiladi { get; set; }
            public string? Shabak { get; set; }
            public string? Nasher { get; set; }
            public string? NobateChap { get; set; }
            public string? Vazeeyat { get; set; }
            public string? CodePeyvast { get; set; }
            public string? SharhPeyvast { get; set; }
        }

        /// <summary>
        /// نتیجه‌ی عملیات BulkUpload.
        /// </summary>
        public class BulkUploadResultDto
        {
            public int InsertedDars { get; set; }
            public int UpdatedDars { get; set; }
            public int InsertedManba { get; set; }
            public int UpdatedManba { get; set; }
            public int DeletedManba { get; set; }
            public int ErrorCount { get; set; }
            public List<BulkUploadErrorDto> Errors { get; set; } = new();
        }

        public class BulkUploadErrorDto
        {
            public int? RowNumber { get; set; }
            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? CodeDars { get; set; }
            public string? ShomareManba { get; set; }
            public string? Stage { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}
