using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Dars;
using PayamBack.Models.Edu;
using System.Linq.Expressions;


namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    public class DarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string AllDarsCacheKey = "AllDarsList";

        public DarsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // ============================================================
        // 1️⃣ دریافت لیست درس‌ها (عمومی) - با فیلتر و صفحه‌بندی
        // ============================================================
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
                // ============================================================
                // تشخیص وجود فیلتر
                // ============================================================
                bool hasAnyFilter = !string.IsNullOrEmpty(search) ||
                                   (reshtehIds?.Length > 0) ||
                                   (grooheAmoozeshiIds?.Length > 0) ||
                                   (maghtas?.Length > 0) ||
                                   (daneshkades?.Length > 0) ||
                                   (vahedTypes?.Length > 0) ||
                                   (termAkhzs?.Length > 0) ||
                                   (noeDarsList?.Length > 0) ||
                                   !string.IsNullOrEmpty(manbaSearch);

                // ============================================================
                // 🔥 متغیر برای نگه‌داری کل داده‌ها (نه فقط یه صفحه)
                // ============================================================
                List<DarsListDto> allData;

                // ============================================================
                // حالت ۱: بدون فیلتر → از کش بخون یا از DB
                // ============================================================
                if (!hasAnyFilter)
                {
                    if (_cache.TryGetValue(AllDarsCacheKey, out List<DarsListDto>? cachedData) && cachedData != null)
                    {
                        // ✅ از کش خوندیم - کل لیست
                        allData = cachedData;
                    }
                    else
                    {
                        // ❌ کش نداشتیم → کل لیست رو از DB بگیر
                        allData = await LoadAllDarsFromDbAsync();

                        // 🔥 کل لیست رو کش کن (نه فقط یه صفحه!)
                        _cache.Set(AllDarsCacheKey, allData, TimeSpan.FromHours(6));
                    }
                }
                // ============================================================
                // حالت ۲: با فیلتر → از DB بخون (فیلترشده)
                // ============================================================
                else
                {
                    allData = await LoadFilteredDarsFromDbAsync(
                        search, reshtehIds, grooheAmoozeshiIds,
                        maghtas, daneshkades, vahedTypes,
                        termAkhzs, noeDarsList, manbaSearch);
                }

                // ============================================================
                // 🔥 صفحه‌بندی توی حافظه
                // ============================================================
                var totalCount = allData.Count;
                var pagedData = allData
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

        // ============================================================
        // 🔥 متد کمکی ۱: دریافت کل لیست از DB (بدون فیلتر)
        // ============================================================
        private async Task<List<DarsListDto>> LoadAllDarsFromDbAsync()
        {
            var darsList = await _context.Dars
                .Include(d => d.Reshteh)
                    .ThenInclude(r => r.GrooheAmoozeshi)
                .OrderBy(d => d.CodeDars)
//                .ThenBy(d => d.NaamDars)
                .Select(d => new
                {
                    Dars = d,
                    ManbaList = _context.ManbaDars
                        .Where(m => m.DarsId == d.Id)
                        .OrderBy(m => m.Id)
                        .Select(m => new
                        {
                            m.Id,
                            m.ShomareManba,
                            m.Onvan,
                            m.CodePeyvast
                        })
                        .ToList()
                })
                .ToListAsync();

            return darsList.Select(x => new DarsListDto
            {
                Id = x.Dars.Id,
                CodeDars = x.Dars.CodeDars,
                NaamDars = x.Dars.NaamDars,
                VahedTeori = x.Dars.VahedTeori,
                VahedAmali = x.Dars.VahedAmali,
                SaatTeoriOrginal = x.Dars.SaatTeoriOrginal,
                SaatAmaliOrginal = x.Dars.SaatAmaliOrginal,
                SaatTeori = x.Dars.SaatTeori,
                SaatAmali = x.Dars.SaatAmali,
                TermAkhz = x.Dars.TermAkhz,
                NoeDars = x.Dars.NoeDars,
                NoeAzmoon = x.Dars.NoeAzmoon,
                ReshtehId = x.Dars.ReshtehId,
                ReshtehName = x.Dars.Reshteh != null ? x.Dars.Reshteh.OnvanReshte : null,
                GrooheAmoozeshiId = x.Dars.Reshteh != null ? x.Dars.Reshteh.GrooheAmoozeshiId : null,
                GrooheName = x.Dars.Reshteh != null && x.Dars.Reshteh.GrooheAmoozeshi != null
                    ? x.Dars.Reshteh.GrooheAmoozeshi.OnvanGrooheAmoozeshi
                    : null,
                Zarfiat = x.Dars.Zarfiat,
                ManbaCount = x.ManbaList.Count,
                ManbaList = x.ManbaList.Select((m, index) => new ManbaDarsSimpleDto
                {
                    Id = m.Id,
                    DarsId = x.Dars.Id,
                    ShomareManba = m.ShomareManba,
                    Onvan = m.Onvan,
                    CodePeyvast = m.CodePeyvast,
                    ManbaIndex = index + 1
                }).ToList()
            }).ToList();
        }

        // ============================================================
        // 🔥 متد کمکی ۲: دریافت لیست فیلترشده از DB
        // ============================================================
        private async Task<List<DarsListDto>> LoadFilteredDarsFromDbAsync(
            string? search,
            int[]? reshtehIds,
            int[]? grooheAmoozeshiIds,
            string[]? maghtas,
            string[]? daneshkades,
            string[]? vahedTypes,
            string[]? termAkhzs,
            string[]? noeDarsList,
            string? manbaSearch)
        {
            var query = _context.Dars
                .Include(d => d.Reshteh)
                    .ThenInclude(r => r.GrooheAmoozeshi)
                .AsQueryable();

            // 🔍 جستجو
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(d =>
                    d.CodeDars.Contains(search) ||
                    d.NaamDars.Contains(search));
            }

            // 🔍 فیلتر رشته
            if (reshtehIds != null && reshtehIds.Length > 0)
            {
                query = query.Where(d => d.ReshtehId.HasValue && reshtehIds.Contains(d.ReshtehId.Value));
            }

            // 🔍 فیلتر گروه آموزشی
            if (grooheAmoozeshiIds != null && grooheAmoozeshiIds.Length > 0)
            {
                query = query.Where(d =>
                    d.Reshteh != null &&
                    d.Reshteh.GrooheAmoozeshiId.HasValue &&
                    grooheAmoozeshiIds.Contains(d.Reshteh.GrooheAmoozeshiId.Value));
            }

            // 🔍 فیلتر مقطع
            if (maghtas != null && maghtas.Length > 0)
            {
                query = query.Where(d =>
                    d.Reshteh != null &&
                    d.Reshteh.CodeMaghta != null &&
                    maghtas.Contains(d.Reshteh.CodeMaghta));
            }

            // 🔍 فیلتر دانشکده
            if (daneshkades != null && daneshkades.Length > 0)
            {
                query = query.Where(d =>
                    d.Reshteh != null &&
                    d.Reshteh.GrooheAmoozeshi != null &&
                    d.Reshteh.GrooheAmoozeshi.CodeDaneshkade != null &&
                    daneshkades.Contains(d.Reshteh.GrooheAmoozeshi.CodeDaneshkade));
            }

            // 🔍 فیلتر واحد درس
            if (vahedTypes != null && vahedTypes.Length > 0)
            {
                query = query.Where(d =>
                    (vahedTypes.Contains("teori") &&
                        (d.VahedTeori ?? 0) > 0 &&
                        (d.VahedAmali ?? 0) == 0) ||
                    (vahedTypes.Contains("amali") &&
                        (d.VahedAmali ?? 0) > 0 &&
                        (d.VahedTeori ?? 0) == 0) ||
                    (vahedTypes.Contains("teori_amali") &&
                        (d.VahedTeori ?? 0) > 0 &&
                        (d.VahedAmali ?? 0) > 0)
                );
            }

            // 🔍 فیلتر ترم اخذ
            if (termAkhzs != null && termAkhzs.Length > 0)
            {
                var includeNull = termAkhzs.Contains("null");
                var termValues = termAkhzs
                    .Where(t => t != "null")
                    .Select(t => int.TryParse(t, out var v) ? v : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .ToList();

                query = query.Where(d =>
                    (includeNull && !d.TermAkhz.HasValue) ||
                    (d.TermAkhz.HasValue && termValues.Contains(d.TermAkhz.Value)));
            }

            // 🔍 فیلتر نوع درس
            if (noeDarsList != null && noeDarsList.Length > 0)
            {
                query = query.Where(d => d.NoeDars != null && noeDarsList.Contains(d.NoeDars));
            }

            // 🔍 فیلتر منبع
            if (!string.IsNullOrEmpty(manbaSearch))
            {
                manbaSearch = manbaSearch.Trim();
                query = query.Where(d =>
                    _context.ManbaDars.Any(m =>
                        m.DarsId == d.Id &&
                        ((m.ShomareManba != null && m.ShomareManba.Contains(manbaSearch)) ||
                         (m.Onvan != null && m.Onvan.Contains(manbaSearch)))));
            }

            // 🔥 کل لیست فیلترشده رو بگیر (بدون Skip/Take)
            var darsList = await query
                .OrderBy(d => d.CodeDars)
                //.ThenBy(d => d.CodeDars)
                .Select(d => new
                {
                    Dars = d,
                    ManbaList = _context.ManbaDars
                        .Where(m => m.DarsId == d.Id)
                        .OrderBy(m => m.Id)
                        .Select(m => new
                        {
                            m.Id,
                            m.ShomareManba,
                            m.Onvan,
                            m.CodePeyvast
                        })
                        .ToList()
                })
                .ToListAsync();

            return darsList.Select(x => new DarsListDto
            {
                Id = x.Dars.Id,
                CodeDars = x.Dars.CodeDars,
                NaamDars = x.Dars.NaamDars,
                VahedTeori = x.Dars.VahedTeori,
                VahedAmali = x.Dars.VahedAmali,
                SaatTeoriOrginal = x.Dars.SaatTeoriOrginal,
                SaatAmaliOrginal = x.Dars.SaatAmaliOrginal,
                SaatTeori = x.Dars.SaatTeori,
                SaatAmali = x.Dars.SaatAmali,
                TermAkhz = x.Dars.TermAkhz,
                NoeDars = x.Dars.NoeDars,
                NoeAzmoon = x.Dars.NoeAzmoon,
                ReshtehId = x.Dars.ReshtehId,
                ReshtehName = x.Dars.Reshteh != null ? x.Dars.Reshteh.OnvanReshte : null,
                GrooheAmoozeshiId = x.Dars.Reshteh != null ? x.Dars.Reshteh.GrooheAmoozeshiId : null,
                GrooheName = x.Dars.Reshteh != null && x.Dars.Reshteh.GrooheAmoozeshi != null
                    ? x.Dars.Reshteh.GrooheAmoozeshi.OnvanGrooheAmoozeshi
                    : null,
                Zarfiat = x.Dars.Zarfiat,
                ManbaCount = x.ManbaList.Count,
                ManbaList = x.ManbaList.Select((m, index) => new ManbaDarsSimpleDto
                {
                    Id = m.Id,
                    DarsId = x.Dars.Id,
                    ShomareManba = m.ShomareManba,
                    Onvan = m.Onvan,
                    CodePeyvast = m.CodePeyvast,
                    ManbaIndex = index + 1
                }).ToList()
            }).ToList();
        }

        //نوع درس
        [HttpGet("noedars-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNoeDarsList()
        {
            try
            {
                const string cacheKey = "NoeDarsList";
                if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
                {
                    return Ok(new { success = true, data = cached });
                }

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

        //منبع درس
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
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dars = await _context.Dars
                    .Include(d => d.Reshteh)
                        .ThenInclude(r => r.GrooheAmoozeshi)
                    .Include(d => d.ManbaDarsList)   // 🔥 منابع
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

                    // 🔥 منابع
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
        // 3️⃣ ایجاد درس جدید
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

                // بررسی وجود رشته
                if (dto.ReshtehId.HasValue)
                {
                    var reshtehExists = await _context.Reshtehs
                        .AnyAsync(r => r.Id == dto.ReshtehId.Value);

                    if (!reshtehExists)
                        return BadRequest(new { success = false, message = "رشته یافت نشد" });
                }

                // بررسی تکراری بودن (کد درس + رشته)
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

                // 🔥 گرفتن ترم جاری
                var currentTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => t.CodeTerm)
                    .FirstOrDefaultAsync();

                // 🔥 ساخت درس با منابع
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
                        TermUpdate = currentTerm,          // 🔥 از ترم جاری
                        CreatedAt = DateTime.Now
                    }).ToList() ?? new List<ManbaDars>()
                };

                await _context.Dars.AddAsync(dars);
                await _context.SaveChangesAsync();

                _cache.Remove(AllDarsCacheKey);

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
        // 4️⃣ ویرایش درس
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

                // 🔥 Include منابع
                var dars = await _context.Dars
                    .Include(d => d.ManbaDarsList)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dars == null)
                    return NotFound(new { success = false, message = "درس یافت نشد" });

                // بررسی وجود رشته
                if (dto.ReshtehId.HasValue)
                {
                    var reshtehExists = await _context.Reshtehs
                        .AnyAsync(r => r.Id == dto.ReshtehId.Value);

                    if (!reshtehExists)
                        return BadRequest(new { success = false, message = "رشته یافت نشد" });
                }

                // بررسی تکراری
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

                // 🔄 به‌روزرسانی فیلدهای درس
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

                // ============================================================
                // 🔥 منابع: Smart Diff (Update موجودها + Insert جدیدها + Delete حذف‌شده‌ها)
                // ============================================================
                var existingManbas = dars.ManbaDarsList?.ToList() ?? new List<ManbaDars>();

                // گرفتن ترم جاری (فقط یک بار)
                var currentTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => t.CodeTerm)
                    .FirstOrDefaultAsync();

                // Idهای ارسال‌شده از فرانت
                var incomingIds = dto.ManbaList?
                    .Where(m => m.Id > 0)
                    .Select(m => m.Id)
                    .ToHashSet() ?? new HashSet<int>();

                // 1️⃣ حذف منابعی که توی DTO نیستن (کاربر حذفشون کرده)
                var manbasToDelete = existingManbas
                    .Where(m => !incomingIds.Contains(m.Id))
                    .ToList();

                if (manbasToDelete.Any())
                {
                    _context.ManbaDars.RemoveRange(manbasToDelete);
                }

                // 2️⃣ به‌روزرسانی منابع موجود + درج منابع جدید
                if (dto.ManbaList != null && dto.ManbaList.Any())
                {
                    foreach (var manbaDto in dto.ManbaList)
                    {
                        if (manbaDto.Id > 0)
                        {
                            // ✅ UPDATE: منبع موجود
                            var existingManba = existingManbas.FirstOrDefault(m => m.Id == manbaDto.Id);
                            if (existingManba != null)
                            {
                                // 🔥 بررسی تغییر واقعی قبل از آپدیت
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

                                // 🔥 فقط اگه واقعاً تغییری بوده، آپدیت کن
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

                                    // 🔥 فقط وقتی تغییر واقعی بوده، TermUpdate و UpdatedAt رو به‌روز کن
                                    existingManba.TermUpdate = currentTerm;
                                    existingManba.UpdatedAt = DateTime.Now;
                                }
                            }
                        }
                        else
                        {
                            // ✅ INSERT: منبع جدید
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
                _cache.Remove(AllDarsCacheKey);

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

                // 🔍 بررسی وابستگی منابع درسی
                var hasManba = await _context.ManbaDars
                    .AnyAsync(m => m.DarsId == id);

                if (hasManba)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این درس دارای منبع است و قابل حذف نیست. ابتدا منابع را حذف کنید"
                    });

                _context.Dars.Remove(dars);
                await _context.SaveChangesAsync();

                // 🔥 پاک کردن کش
                _cache.Remove(AllDarsCacheKey);

                return Ok(new
                {
                    success = true,
                    message = "درس با موفقیت حذف شد"
                });
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
        public IActionResult ClearCache()
        {
            _cache.Remove(AllDarsCacheKey);
            return Ok(new { success = true, message = "کش درس‌ها پاک شد" });
        }

        // ============================================================
        // 7️⃣ آپلود گروهی درس‌ها و منابع از فایل اکسل (گزارش ۱۰۰۹)
        // ============================================================
        [HttpPost("bulk-upload")]
        [Authorize]
        [RequestSizeLimit(50_000_000)] // 50MB
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
                // ۲. گروه‌بندی رکوردها بر اساس Reshteh
                // ============================================================
                var groupedByReshteh = GroupByReshteh(rows);

                // ============================================================
                // ۳. پردازش هر رشته جداگانه
                // ============================================================
                var rejectedReshtehs = new List<RejectedReshtehDto>();
                int totalDarsCreated = 0;
                int totalManbaCreated = 0;

                foreach (var reshtehGroup in groupedByReshteh)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // 🔍 پیدا کردن رشته
                        var reshteh = await _context.Reshtehs
                            .FirstOrDefaultAsync(r =>
                                r.CodeReshte == reshtehGroup.CodeReshte &&
                                r.CodeMaghta == reshtehGroup.CodeMaghta &&
                                r.TermVorood == reshtehGroup.TermVorood &&
                                r.TermEamal == reshtehGroup.TermEamal);

                        if (reshteh == null)
                        {
                            await transaction.RollbackAsync();
                            rejectedReshtehs.Add(new RejectedReshtehDto
                            {
                                ReshtehName = reshtehGroup.ReshtehName,
                                CodeReshte = reshtehGroup.CodeReshte,
                                CodeMaghta = reshtehGroup.CodeMaghta,
                                TermVorood = reshtehGroup.TermVorood,
                                TermEamal = reshtehGroup.TermEamal,
                                Reason = "رشته در سیستم یافت نشد"
                            });
                            continue;
                        }

                        // 🔍 چک تکراری بودن درس‌ها (توی همین رشته)
                        var codeDarsList = reshtehGroup.DarsList
                            .Select(d => d.CodeDars)
                            .ToList();

                        var existingDars = await _context.Dars
                            .Where(d => d.ReshtehId == reshteh.Id && codeDarsList.Contains(d.CodeDars))
                            .Select(d => d.CodeDars)
                            .ToListAsync();

                        if (existingDars.Any())
                        {
                            await transaction.RollbackAsync();
                            rejectedReshtehs.Add(new RejectedReshtehDto
                            {
                                ReshtehName = reshtehGroup.ReshtehName,
                                CodeReshte = reshtehGroup.CodeReshte,
                                CodeMaghta = reshtehGroup.CodeMaghta,
                                TermVorood = reshtehGroup.TermVorood,
                                TermEamal = reshtehGroup.TermEamal,
                                Reason = $"دروس تکراری: {string.Join(", ", existingDars)}"
                            });
                            continue;
                        }

                        // ✅ ثبت درس‌ها و منابع
                        int darsCreated = 0;
                        int manbaCreated = 0;

                        foreach (var darsGroup in reshtehGroup.DarsList)
                        {
                            // ساخت درس با منابع
                            var dars = new Dars
                            {
                                CodeDars = darsGroup.CodeDars,
                                NaamDars = darsGroup.NaamDars,
                                VahedTeori = darsGroup.VahedTeori,
                                VahedAmali = darsGroup.VahedAmali,
                                SaatTeoriOrginal = darsGroup.SaatTeoriOrginal,
                                SaatAmaliOrginal = darsGroup.SaatAmaliOrginal,
                                SaatTeori = darsGroup.SaatTeori,
                                SaatAmali = darsGroup.SaatAmali,
                                TermAkhz = darsGroup.TermAkhz,
                                NoeDars = darsGroup.NoeDars,
                                NoeAzmoon = darsGroup.NoeAzmoon,
                                ReshtehId = reshteh.Id,
                                Zarfiat = darsGroup.Zarfiat,
                                CreatedAt = DateTime.Now,
                                ManbaDarsList = darsGroup.ManbaList.Select(m => new ManbaDars
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
                                    TermUpdate = m.TermUpdate,
                                    CreatedAt = DateTime.Now
                                }).ToList()
                            };

                            await _context.Dars.AddAsync(dars);
                            darsCreated++;
                            manbaCreated += darsGroup.ManbaList.Count;
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        totalDarsCreated += darsCreated;
                        totalManbaCreated += manbaCreated;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        rejectedReshtehs.Add(new RejectedReshtehDto
                        {
                            ReshtehName = reshtehGroup.ReshtehName,
                            CodeReshte = reshtehGroup.CodeReshte,
                            CodeMaghta = reshtehGroup.CodeMaghta,
                            TermVorood = reshtehGroup.TermVorood,
                            TermEamal = reshtehGroup.TermEamal,
                            Reason = $"خطای غیرمنتظره: {ex.Message}"
                        });
                    }
                }

                // ============================================================
                // ۴. پاک کردن کش
                // ============================================================
                if (totalDarsCreated > 0)
                {
                    _cache.Remove(AllDarsCacheKey);
                }

                // ============================================================
                // ۵. ساخت فایل اکسل خطاها (اگه رشته ردشده وجود داشت)
                // ============================================================
                string? errorFileBase64 = null;
                if (rejectedReshtehs.Any())
                {
                    errorFileBase64 = GenerateErrorExcel(rejectedReshtehs);
                }

                return Ok(new
                {
                    success = true,
                    message = $"تعداد {totalDarsCreated} درس و {totalManbaCreated} منبع ثبت شد. " +
                              $"{rejectedReshtehs.Count} رشته رد شد.",
                    data = new
                    {
                        darsCreated = totalDarsCreated,
                        manbaCreated = totalManbaCreated,
                        rejectedReshtehCount = rejectedReshtehs.Count,
                        rejectedReshtehs = rejectedReshtehs
                    },
                    errorFile = errorFileBase64,
                    errorFileName = rejectedReshtehs.Any() ? "rejected-reshtehs.xlsx" : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در پردازش فایل",
                    error = ex.Message
                });
            }
        }

        private List<DarsImportRow> ParseExcelFile(Stream stream)
        {
            var rows = new List<DarsImportRow>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++) // از ردیف ۲ (چون ۱ هدره)
            {
                var importRow = new DarsImportRow
                {
                    RowNumber = row,
                    CodeDars = GetStringValue(worksheet, row, 4),
                    NaamDars = GetStringValue(worksheet, row, 5),
                    VahedTeori = GetDecimalValue(worksheet, row, 6),
                    VahedAmali = GetDecimalValue(worksheet, row, 7),
                    SaatTeoriOrginal = GetIntValue(worksheet, row, 8),
                    SaatAmaliOrginal = GetIntValue(worksheet, row, 9),
                    SaatTeori = GetIntValue(worksheet, row, 8),
                    SaatAmali = GetIntValue(worksheet, row, 9),
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

        // توابع کمکی برای خواندن مقادیر
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

        private List<ReshtehGroup> GroupByReshteh(List<DarsImportRow> rows)
        {
            var result = new List<ReshtehGroup>();
            ReshtehGroup? currentReshteh = null;
            DarsWithManba? currentDars = null;

            foreach (var row in rows)
            {
                // 🔍 آیا رشته جدید است؟
                var isNewReshteh = currentReshteh == null ||
                    currentReshteh.CodeReshte != row.CodeReshte ||
                    currentReshteh.CodeMaghta != row.CodeMaghta ||
                    currentReshteh.TermVorood != row.TermVorood ||
                    currentReshteh.TermEamal != row.TermEamal;

                if (isNewReshteh)
                {
                    currentReshteh = new ReshtehGroup
                    {
                        CodeReshte = row.CodeReshte,
                        CodeMaghta = row.CodeMaghta,
                        TermVorood = row.TermVorood,
                        TermEamal = row.TermEamal,
                        ReshtehName = row.ReshtehName,
                        DarsList = new List<DarsWithManba>()
                    };
                    result.Add(currentReshteh);
                    currentDars = null;
                }

                // 🔍 آیا درس جدید است؟ (ستون ۴ پر باشه)
                if (!string.IsNullOrEmpty(row.CodeDars))
                {
                    currentDars = new DarsWithManba
                    {
                        CodeDars = row.CodeDars,
                        NaamDars = row.NaamDars,
                        VahedTeori = row.VahedTeori,
                        VahedAmali = row.VahedAmali,
                        SaatTeoriOrginal = row.SaatTeoriOrginal,
                        SaatAmaliOrginal = row.SaatAmaliOrginal,
                        SaatTeori = row.SaatTeori,
                        SaatAmali = row.SaatAmali,
                        TermAkhz = row.TermAkhz,
                        NoeDars = row.NoeDars,
                        NoeAzmoon = row.NoeAzmoon,
                        Zarfiat = row.Zarfiat,
                        ManbaList = new List<ManbaImportRow>()
                    };

                    // منبع اول (اگه هست)
                    if (!string.IsNullOrEmpty(row.OnvanManba))
                    {
                        currentDars.ManbaList.Add(CreateManbaFromRow(row));
                    }

                    currentReshteh.DarsList.Add(currentDars);
                }
                // 🔍 منبع اضافی برای درس قبلی
                else if (currentDars != null)
                {
                    if (!string.IsNullOrEmpty(row.OnvanManba))
                    {
                        currentDars.ManbaList.Add(CreateManbaFromRow(row));
                    }
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
                SharhPeyvast = row.SharhPeyvast,
                TermUpdate = row.TermUpdate
            };
        }

        private string GenerateErrorExcel(List<RejectedReshtehDto> rejectedReshtehs)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ردشده‌ها");

            // هدر
            worksheet.Cell(1, 1).Value = "ردیف";
            worksheet.Cell(1, 2).Value = "نام رشته";
            worksheet.Cell(1, 3).Value = "کد رشته";
            worksheet.Cell(1, 4).Value = "کد مقطع";
            worksheet.Cell(1, 5).Value = "ترم ورود";
            worksheet.Cell(1, 6).Value = "ترم اعمال";
            worksheet.Cell(1, 7).Value = "دلیل خطا";

            // استایل هدر
            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // داده‌ها
            int rowNum = 2;
            foreach (var item in rejectedReshtehs)
            {
                worksheet.Cell(rowNum, 1).Value = rowNum - 1;
                worksheet.Cell(rowNum, 2).Value = item.ReshtehName ?? "-";
                worksheet.Cell(rowNum, 3).Value = item.CodeReshte ?? "-";
                worksheet.Cell(rowNum, 4).Value = item.CodeMaghta ?? "-";
                worksheet.Cell(rowNum, 5).Value = item.TermVorood ?? "-";
                worksheet.Cell(rowNum, 6).Value = item.TermEamal ?? "-";
                worksheet.Cell(rowNum, 7).Value = item.Reason ?? "-";
                rowNum++;
            }

            // تنظیم عرض ستون‌ها
            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 12;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 50;

            // تبدیل به Base64
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private class DarsImportRow
        {
            public int RowNumber { get; set; }
            public string? CodeDars { get; set; }       // ستون 4
            public string? NaamDars { get; set; }        // ستون 5
            public decimal? VahedTeori { get; set; }     // ستون 6
            public decimal? VahedAmali { get; set; }     // ستون 7
            public int? SaatTeoriOrginal { get; set; }   // ستون 8
            public int? SaatAmaliOrginal { get; set; }   // ستون 9
            public int? SaatTeori { get; set; }          // ستون 8
            public int? SaatAmali { get; set; }          // ستون 9
            public int? TermAkhz { get; set; }           // ستون 10
            public string? NoeDars { get; set; }         // ستون 11
            public string? NoeAzmoon { get; set; }       // ستون 12
            public int? Zarfiat { get; set; }            // ستون 28
            public string? TermUpdate { get; set; }      // ستون 3

            // رشته
            public string? CodeReshte { get; set; }      // ستون 26
            public string? CodeMaghta { get; set; }      // ستون 27
            public string? TermVorood { get; set; }      // ستون 37
            public string? TermEamal { get; set; }       // ستون 38
            public string? ReshtehName { get; set; }     // ستون 34

            // منبع
            public string? ShomareManba { get; set; }    // ستون 13
            public string? NoeManba { get; set; }        // ستون 14
            public string? OnvanManba { get; set; }      // ستون 15
            public string? Nevisandeh { get; set; }      // ستون 16
            public string? Motarjem { get; set; }        // ستون 17
            public string? SalEnteshar { get; set; }     // ستون 18
            public string? SalEntesharMiladi { get; set; } // ستون 19
            public string? Shabak { get; set; }          // ستون 20
            public string? Nasher { get; set; }          // ستون 21
            public string? NobateChap { get; set; }      // ستون 29
            public string? Vazeeyat { get; set; }        // ستون 22
            public string? CodePeyvast { get; set; }     // ستون 23
            public string? SharhPeyvast { get; set; }    // ستون 24
        }

        private class DarsWithManba
        {
            public string CodeDars { get; set; } = string.Empty;
            public string? NaamDars { get; set; }
            public decimal? VahedTeori { get; set; }
            public decimal? VahedAmali { get; set; }
            public int? SaatTeoriOrginal { get; set; }
            public int? SaatAmaliOrginal { get; set; }
            public int? SaatTeori { get; set; }
            public int? SaatAmali { get; set; }
            public int? TermAkhz { get; set; }
            public string? NoeDars { get; set; }
            public string? NoeAzmoon { get; set; }
            public int? Zarfiat { get; set; }
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
            public string? TermUpdate { get; set; }
        }

        private class ReshtehGroup
        {
            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? ReshtehName { get; set; }
            public List<DarsWithManba> DarsList { get; set; } = new();
        }

        public class RejectedReshtehDto
        {
            public string? ReshtehName { get; set; }
            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? Reason { get; set; }
        }


    }


}
