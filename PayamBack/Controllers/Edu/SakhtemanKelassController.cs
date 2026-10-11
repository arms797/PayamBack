// PayamBack/Controllers/Edu/SakhtemanKelassController.cs
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Edu.SakhtemanKelass;
using PayamBack.Models.Edu;
using PayamBack.Services.Interfaces;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SakhtemanKelassController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAccessService _accessService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMarkazCacheService _markazCache;


        public SakhtemanKelassController(
            AppDbContext context,
            IAccessService accessService,
            ICurrentUserService currentUserService,
            IMarkazCacheService markazCache)
        {
            _context = context;
            _accessService = accessService;
            _currentUserService = currentUserService;
            _markazCache = markazCache;

        }

        // ============================================================
        // 1️⃣ لیست با فیلتر و صفحه‌بندی
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? codeOstan = null,
            [FromQuery] int? markazId = null,
            [FromQuery] string? searchNaamSakhteman = null,
            [FromQuery] string? searchNaamClass = null,
            [FromQuery] bool? amoozeshi = null,
            [FromQuery] bool? vazeeyat = null,
            [FromQuery] bool? projector = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await _accessService
                    .GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new
                    {
                        success = true,
                        message = "شما دسترسی به هیچ مرکزی ندارید",
                        data = new List<object>(),
                        pagination = new { page, pageSize, totalCount = 0, totalPages = 0 }
                    });

                var query = _context.SakhtemanKelasses
                    .Include(s => s.Markaz)
                    .Where(s => accessibleMarkazIds.Contains(s.MarkazId))
                    .AsQueryable();

                // فیلتر استان
                if (!string.IsNullOrEmpty(codeOstan))
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == codeOstan)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(s => markazIdsInOstan.Contains(s.MarkazId));
                }

                // فیلتر مرکز
                if (markazId.HasValue && markazId.Value > 0)
                    query = query.Where(s => s.MarkazId == markazId.Value);

                // جستجو نام ساختمان
                if (!string.IsNullOrEmpty(searchNaamSakhteman))
                {
                    var search = searchNaamSakhteman.Trim();
                    query = query.Where(s => s.NaamSakhteman.Contains(search));
                }

                // جستجو نام کلاس
                if (!string.IsNullOrEmpty(searchNaamClass))
                {
                    var search = searchNaamClass.Trim();
                    query = query.Where(s => s.NaamClass.Contains(search));
                }

                // فیلتر آموزش/اداری
                if (amoozeshi.HasValue)
                    query = query.Where(s => s.Amoozeshi == amoozeshi.Value);

                // فیلتر وضعیت
                if (vazeeyat.HasValue)
                    query = query.Where(s => s.Vazeeyat == vazeeyat.Value);

                // فیلتر پروژکتور
                if (projector.HasValue)
                    query = query.Where(s => s.Projector == projector.Value);

                // مرتب‌سازی ثابت
                query = query
                    .OrderBy(s => s.NaamSakhteman)
                    .ThenBy(s => s.NaamClass);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SakhtemanKelassListDto
                    {
                        Id = s.Id,
                        MarkazId = s.MarkazId,
                        MarkazName = s.Markaz != null ? s.Markaz.NaamMarkaz : "",
                        NaamSakhteman = s.NaamSakhteman,
                        NaamClass = s.NaamClass,
                        Amoozeshi = s.Amoozeshi,
                        Vazeeyat = s.Vazeeyat,
                        Projector = s.Projector,
                        TedadWhiteboard = s.TedadWhiteboard
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست ساختمان‌ها دریافت شد",
                    data = items,
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
                    message = "خطا در دریافت لیست ساختمان‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک ساختمان/کلاس
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.SakhtemanKelasses
                    .Include(s => s.Markaz)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "ساختمان/کلاس یافت نشد" });

                if (!await _accessService.CanAccessTargetMarkazAsync(
                    entity.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var dto = new SakhtemanKelassDetailDto
                {
                    Id = entity.Id,
                    MarkazId = entity.MarkazId,
                    MarkazName = entity.Markaz?.NaamMarkaz ?? "",
                    CodeSakhteman = entity.CodeSakhteman,
                    NaamSakhteman = entity.NaamSakhteman,
                    CodeClass = entity.CodeClass,
                    NaamClass = entity.NaamClass,
                    NoeClass = entity.NoeClass,
                    Vazeeyat = entity.Vazeeyat,
                    Zarfiat = entity.Zarfiat,
                    Tabagheh = entity.Tabagheh,
                    ZarfiatEmtahani = entity.ZarfiatEmtahani,
                    TedadWhiteboard = entity.TedadWhiteboard,
                    Projector = entity.Projector,
                    Emkanat = entity.Emkanat,
                    Tozihat = entity.Tozihat,
                    Telephon = entity.Telephon,
                    Amoozeshi = entity.Amoozeshi
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات ساختمان دریافت شد",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات ساختمان",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد ساختمان/کلاس جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SakhtemanKelassCreateDto dto)
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

                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (!await _accessService.CanAccessTargetMarkazAsync(
                    dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                var markazExists = await _context.Markazes
                    .AnyAsync(m => m.Id == dto.MarkazId);
                if (!markazExists)
                    return BadRequest(new { success = false, message = "مرکز یافت نشد" });

                var exists = await _context.SakhtemanKelasses
                    .AnyAsync(s =>
                        s.MarkazId == dto.MarkazId &&
                        s.CodeSakhteman == dto.CodeSakhteman &&
                        s.CodeClass == dto.CodeClass);

                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"کلاسی با کد «{dto.CodeClass}» در این ساختمان قبلاً ثبت شده است"
                    });
                }

                var entity = new SakhtemanKelass
                {
                    MarkazId = dto.MarkazId,
                    CodeSakhteman = dto.CodeSakhteman,
                    NaamSakhteman = dto.NaamSakhteman.Trim(),
                    CodeClass = dto.CodeClass,
                    NaamClass = dto.NaamClass.Trim(),
                    NoeClass = dto.NoeClass?.Trim(),
                    Vazeeyat = dto.Vazeeyat,
                    Zarfiat = dto.Zarfiat,
                    Tabagheh = dto.Tabagheh,
                    ZarfiatEmtahani = dto.ZarfiatEmtahani,
                    TedadWhiteboard = dto.TedadWhiteboard,
                    Projector = dto.Projector,
                    Emkanat = dto.Emkanat?.Trim(),
                    Tozihat = dto.Tozihat?.Trim(),
                    Telephon = dto.Telephon?.Trim(),
                    Amoozeshi = dto.Amoozeshi,
                    CreatedAt = DateTime.Now
                };

                await _context.SakhtemanKelasses.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ساختمان/کلاس با موفقیت ایجاد شد",
                    data = new { id = entity.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد ساختمان/کلاس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش ساختمان/کلاس
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SakhtemanKelassUpdateDto dto)
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

                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.SakhtemanKelasses
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "ساختمان/کلاس یافت نشد" });

                if (!await _accessService.CanAccessTargetMarkazAsync(
                    entity.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // بررسی تغییر مرکز
                if (dto.MarkazId.HasValue && dto.MarkazId.Value != entity.MarkazId)
                {
                    if (!await _accessService.CanAccessTargetMarkazAsync(
                        dto.MarkazId.Value, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();

                    var markazExists = await _context.Markazes
                        .AnyAsync(m => m.Id == dto.MarkazId.Value);
                    if (!markazExists)
                        return BadRequest(new { success = false, message = "مرکز جدید یافت نشد" });
                }

                // بررسی تکراری
                var targetMarkazId = dto.MarkazId ?? entity.MarkazId;
                var targetCodeSakhteman = dto.CodeSakhteman ?? entity.CodeSakhteman;
                var targetCodeClass = dto.CodeClass ?? entity.CodeClass;

                var exists = await _context.SakhtemanKelasses
                    .AnyAsync(s =>
                        s.Id != id &&
                        s.MarkazId == targetMarkazId &&
                        s.CodeSakhteman == targetCodeSakhteman &&
                        s.CodeClass == targetCodeClass);

                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "کلاس دیگری با این کد در این ساختمان وجود دارد"
                    });
                }

                // به‌روزرسانی فیلدها
                if (dto.MarkazId.HasValue) entity.MarkazId = dto.MarkazId.Value;
                if (dto.CodeSakhteman.HasValue) entity.CodeSakhteman = dto.CodeSakhteman.Value;
                if (!string.IsNullOrWhiteSpace(dto.NaamSakhteman)) entity.NaamSakhteman = dto.NaamSakhteman.Trim();
                if (dto.CodeClass.HasValue) entity.CodeClass = dto.CodeClass.Value;
                if (!string.IsNullOrWhiteSpace(dto.NaamClass)) entity.NaamClass = dto.NaamClass.Trim();
                if (dto.NoeClass != null) entity.NoeClass = dto.NoeClass.Trim();
                if (dto.Vazeeyat.HasValue) entity.Vazeeyat = dto.Vazeeyat.Value;
                if (dto.Zarfiat.HasValue) entity.Zarfiat = dto.Zarfiat;
                if (dto.Tabagheh.HasValue) entity.Tabagheh = dto.Tabagheh;
                if (dto.ZarfiatEmtahani.HasValue) entity.ZarfiatEmtahani = dto.ZarfiatEmtahani;
                if (dto.TedadWhiteboard.HasValue) entity.TedadWhiteboard = dto.TedadWhiteboard;
                if (dto.Projector.HasValue) entity.Projector = dto.Projector.Value;
                if (dto.Emkanat != null) entity.Emkanat = dto.Emkanat.Trim();
                if (dto.Tozihat != null) entity.Tozihat = dto.Tozihat.Trim();
                if (dto.Telephon != null) entity.Telephon = dto.Telephon.Trim();
                if (dto.Amoozeshi.HasValue) entity.Amoozeshi = dto.Amoozeshi.Value;

                entity.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ساختمان/کلاس با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش ساختمان/کلاس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف ساختمان/کلاس
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.SakhtemanKelasses
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "ساختمان/کلاس یافت نشد" });

                if (!await _accessService.CanAccessTargetMarkazAsync(
                    entity.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                _context.SakhtemanKelasses.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ساختمان/کلاس با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف ساختمان/کلاس",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ تغییر وضعیت (فعال/غیرفعال)
        // ============================================================
        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) =
                    await _currentUserService.GetCurrentUserInfoAsync();

                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var entity = await _context.SakhtemanKelasses
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "ساختمان/کلاس یافت نشد" });

                if (!await _accessService.CanAccessTargetMarkazAsync(
                    entity.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                entity.Vazeeyat = !entity.Vazeeyat;
                entity.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"وضعیت با موفقیت به {(entity.Vazeeyat ? "فعال" : "غیرفعال")} تغییر یافت",
                    data = new { id = entity.Id, vazeeyat = entity.Vazeeyat }
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
        // 7️⃣ آپلود گروهی از فایل اکسل
        // ============================================================
        [HttpPost("bulk-upload")]
        [RequestSizeLimit(50_000_000)] // 50MB
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            // ============================================================
            // 0️⃣ بررسی فایل ورودی
            // ============================================================
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "فایلی انتخاب نشده است" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { success = false, message = "فقط فایل‌های Excel مجاز هستند" });

            // ============================================================
            // 1️⃣ بررسی دسترسی کاربر
            // ============================================================
            var (currentUser, currentRole, currentMarkaz, codeRole) =
                await _currentUserService.GetCurrentUserInfoAsync();

            if (currentUser == null || codeRole == null)
                return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

            var accessibleMarkazIds = await _accessService
                .GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

            if (!accessibleMarkazIds.Any())
                return BadRequest(new { success = false, message = "شما دسترسی به هیچ مرکزی ندارید" });

            try
            {
                // ============================================================
                // 2️⃣ خواندن فایل اکسل
                // ============================================================
                List<SakhtemanKelassImportRow> rows;
                try
                {
                    using var stream = file.OpenReadStream();
                    rows = ParseSakhtemanKelassExcel(stream);
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
                // 3️⃣ گرفتن مراکز از کش
                // ============================================================
                var allMarkazes = await _markazCache.GetAllIncludingInactiveAsync();

                // دیکشنری: CodeMarkaz → MarkazId
                var markazDict = allMarkazes
                    .Where(m => !string.IsNullOrEmpty(m.CodeMarkaz))
                    .GroupBy(m => m.CodeMarkaz!)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                // ============================================================
                // 4️⃣ پردازش هر ردیف
                // ============================================================
                var rejectedRows = new List<(int RowNumber, List<string> RowData, string ErrorMessage)>();
                var toInsert = new List<SakhtemanKelass>();
                var skippedCount = 0;

                // جلوگیری از تکراری در همان فایل
                var seenInFile = new HashSet<string>();

                foreach (var row in rows)
                {
                    // ستون‌های اجباری
                    if (string.IsNullOrEmpty(row.CodeMarkaz) ||
                        string.IsNullOrEmpty(row.CodeSakhteman) ||
                        string.IsNullOrEmpty(row.CodeClass) ||
                        string.IsNullOrEmpty(row.NaamSakhteman) ||
                        string.IsNullOrEmpty(row.NaamClass))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            "کد مرکز، کد ساختمان، نام ساختمان، کد کلاس و نام کلاس اجباری هستند"));
                        continue;
                    }

                    // 1️⃣ پیدا کردن مرکز
                    if (!markazDict.TryGetValue(row.CodeMarkaz.Trim(), out int markazId))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"کد مرکز «{row.CodeMarkaz}» در سیستم یافت نشد"));
                        continue;
                    }

                    // 2️⃣ بررسی دسترسی به مرکز
                    if (!accessibleMarkazIds.Contains(markazId))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"شما به مرکز با کد «{row.CodeMarkaz}» دسترسی ندارید"));
                        continue;
                    }

                    // 3️⃣ Parse اعداد اجباری
                    if (!int.TryParse(row.CodeSakhteman.Trim(), out int codeSakhteman))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"کد ساختمان «{row.CodeSakhteman}» معتبر نیست"));
                        continue;
                    }

                    if (!int.TryParse(row.CodeClass.Trim(), out int codeClass))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"کد کلاس «{row.CodeClass}» معتبر نیست"));
                        continue;
                    }

                    // 4️⃣ Parse وضعیت (ستون 7): 1=فعال, 0=غیرفعال
                    bool vazeeyat;
                    if (string.IsNullOrEmpty(row.VazeeyatRaw))
                    {
                        vazeeyat = true; // پیش‌فرض فعال
                    }
                    else if (row.VazeeyatRaw.Trim() == "1")
                    {
                        vazeeyat = true;
                    }
                    else if (row.VazeeyatRaw.Trim() == "0")
                    {
                        vazeeyat = false;
                    }
                    else
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"وضعیت «{row.VazeeyatRaw}» باید 0 یا 1 باشد"));
                        continue;
                    }

                    // 5️⃣ Parse ویدئو پروژکتور (ستون 12): 1=دارد, 0=ندارد
                    bool projector = false;
                    if (!string.IsNullOrEmpty(row.ProjectorRaw))
                    {
                        if (row.ProjectorRaw.Trim() == "1")
                            projector = true;
                        else if (row.ProjectorRaw.Trim() == "0")
                            projector = false;
                        else
                        {
                            rejectedRows.Add((row.RowNumber, GetRowData(row),
                                $"ویدئو پروژکتور «{row.ProjectorRaw}» باید 0 یا 1 باشد"));
                            continue;
                        }
                    }

                    // 6️⃣ Parse آموزشی (ستون 16): 1=آموزشی, 0=اداری
                    bool? amoozeshi = null;
                    if (!string.IsNullOrEmpty(row.AmoozeshiRaw))
                    {
                        if (row.AmoozeshiRaw.Trim() == "1")
                            amoozeshi = true;
                        else if (row.AmoozeshiRaw.Trim() == "0")
                            amoozeshi = false;
                        else
                        {
                            rejectedRows.Add((row.RowNumber, GetRowData(row),
                                $"آموزشی «{row.AmoozeshiRaw}» باید 0 یا 1 باشد"));
                            continue;
                        }
                    }

                    // 7️⃣ Parse اعداد اختیاری
                    int? zarfiat = ParseNullableInt(row.Zarfiat);
                    int? tabagheh = ParseNullableInt(row.Tabagheh);
                    int? zarfiatEmtahani = ParseNullableInt(row.ZarfiatEmtahani);
                    int? tedadWhiteboard = ParseNullableInt(row.TedadWhiteboard);

                    // 8️⃣ بررسی تکراری در همان فایل
                    var fileKey = $"{markazId}_{codeSakhteman}_{codeClass}";
                    if (seenInFile.Contains(fileKey))
                    {
                        rejectedRows.Add((row.RowNumber, GetRowData(row),
                            $"این کلاس (مرکز {markazId}، ساختمان {codeSakhteman}، کلاس {codeClass}) در همین فایل تکراری است"));
                        continue;
                    }

                    // 9️⃣ بررسی تکراری در دیتابیس → Skip (بدون خطا)
                    var existsInDb = await _context.SakhtemanKelasses
                        .AnyAsync(s =>
                            s.MarkazId == markazId &&
                            s.CodeSakhteman == codeSakhteman &&
                            s.CodeClass == codeClass);

                    if (existsInDb)
                    {
                        skippedCount++;
                        continue;
                    }

                    // 🔟 ساخت رکورد جدید
                    var entity = new SakhtemanKelass
                    {
                        MarkazId = markazId,
                        CodeSakhteman = codeSakhteman,
                        NaamSakhteman = row.NaamSakhteman.Trim(),
                        CodeClass = codeClass,
                        NaamClass = row.NaamClass.Trim(),
                        NoeClass = string.IsNullOrWhiteSpace(row.NoeClass) ? null : row.NoeClass.Trim(),
                        Vazeeyat = vazeeyat,
                        Zarfiat = zarfiat,
                        Tabagheh = tabagheh,
                        ZarfiatEmtahani = zarfiatEmtahani,
                        TedadWhiteboard = tedadWhiteboard,
                        Projector = projector,
                        Emkanat = string.IsNullOrWhiteSpace(row.Emkanat) ? null : row.Emkanat.Trim(),
                        Tozihat = string.IsNullOrWhiteSpace(row.Tozihat) ? null : row.Tozihat.Trim(),
                        Telephon = string.IsNullOrWhiteSpace(row.ResponsiblePhone) ? null : row.ResponsiblePhone.Trim(),
                        Amoozeshi = amoozeshi,
                        CreatedAt = DateTime.Now
                    };

                    toInsert.Add(entity);
                    seenInFile.Add(fileKey);
                }

                // ============================================================
                // 5️⃣ ذخیره در دیتابیس
                // ============================================================
                int insertedCount = 0;
                if (toInsert.Any())
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await _context.SakhtemanKelasses.AddRangeAsync(toInsert);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        insertedCount = toInsert.Count;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "خطا در ذخیره رکوردها",
                            error = ex.Message
                        });
                    }
                }

                // ============================================================
                // 6️⃣ ساخت فایل اکسل خطاها (اگر خطایی هست)
                // ============================================================
                string? errorFileBase64 = null;
                if (rejectedRows.Any())
                {
                    errorFileBase64 = GenerateSakhtemanKelassErrorExcel(rejectedRows);
                }

                // ============================================================
                // 7️⃣ پاسخ نهایی
                // ============================================================
                return Ok(new
                {
                    success = true,
                    message = $"تعداد {insertedCount} رکورد ثبت شد، {skippedCount} رکورد تکراری Skip شد، {rejectedRows.Count} رکورد خطا داشت.",
                    data = new
                    {
                        inserted = insertedCount,
                        skipped = skippedCount,
                        rejected = rejectedRows.Count,
                        total = rows.Count
                    },
                    errorFile = errorFileBase64,
                    errorFileName = rejectedRows.Any() ? "sakhteman-kelass-errors.xlsx" : null
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

        // ============================================================
        // 🔥 متدهای کمکی BulkUpload
        // ============================================================

        /// <summary>
        /// خواندن فایل اکسل و تبدیل به لیست ردیف‌ها
        /// </summary>
        /// <summary>
        /// خواندن فایل اکسل و تبدیل به لیست ردیف‌ها
        /// </summary>
        private List<SakhtemanKelassImportRow> ParseSakhtemanKelassExcel(Stream stream)
        {
            var rows = new List<SakhtemanKelassImportRow>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            // شروع از ردیف 1 (چون هدر نداره)
            for (int row = 1; row <= lastRow; row++)
            {
                var importRow = new SakhtemanKelassImportRow
                {
                    RowNumber = row,
                    CodeMarkaz = GetCellString(worksheet, row, 1),
                    CodeSakhteman = GetCellString(worksheet, row, 2),
                    NaamSakhteman = GetCellString(worksheet, row, 3),
                    CodeClass = GetCellString(worksheet, row, 4),
                    NaamClass = GetCellString(worksheet, row, 5),
                    NoeClass = GetCellString(worksheet, row, 6),
                    VazeeyatRaw = GetCellString(worksheet, row, 7),
                    Zarfiat = GetCellString(worksheet, row, 8),
                    Tabagheh = GetCellString(worksheet, row, 9),
                    ZarfiatEmtahani = GetCellString(worksheet, row, 10),
                    TedadWhiteboard = GetCellString(worksheet, row, 11),
                    ProjectorRaw = GetCellString(worksheet, row, 12),
                    Emkanat = GetCellString(worksheet, row, 13),
                    Tozihat = GetCellString(worksheet, row, 14),
                    ResponsiblePhone = GetCellString(worksheet, row, 15),
                    AmoozeshiRaw = GetCellString(worksheet, row, 16)
                };

                // فقط ردیف‌هایی که کد مرکز یا کد ساختمان دارن
                if (!string.IsNullOrEmpty(importRow.CodeMarkaz) ||
                    !string.IsNullOrEmpty(importRow.CodeSakhteman))
                {
                    rows.Add(importRow);
                }
            }

            return rows;
        }

        /// <summary>
        /// خواندن مقدار string از سلول
        /// </summary>
        private string? GetCellString(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            var value = cell.GetString()?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// تبدیل امن به int?
        /// </summary>
        private int? ParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return int.TryParse(value.Trim(), out int result) ? result : null;
        }

        /// <summary>
        /// گرفتن داده‌های ردیف برای فایل خطا
        /// </summary>
        private List<string> GetRowData(SakhtemanKelassImportRow row)
        {
            return new List<string>
            {
                row.CodeMarkaz ?? "",
                row.CodeSakhteman ?? "",
                row.NaamSakhteman ?? "",
                row.CodeClass ?? "",
                row.NaamClass ?? "",
                row.NoeClass ?? "",
                row.VazeeyatRaw ?? "",
                row.Zarfiat ?? "",
                row.Tabagheh ?? "",
                row.ZarfiatEmtahani ?? "",
                row.TedadWhiteboard ?? "",
                row.ProjectorRaw ?? "",
                row.Emkanat ?? "",
                row.Tozihat ?? "",
                row.ResponsiblePhone ?? "",
                row.AmoozeshiRaw ?? ""
            };
        }

        /// <summary>
        /// ساخت فایل اکسل خطاها
        /// </summary>
        /// <summary>
        /// ساخت فایل اکسل خطاها
        /// </summary>
        private string GenerateSakhtemanKelassErrorExcel(
            List<(int RowNumber, List<string> RowData, string ErrorMessage)> rejectedRows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("خطاها");

            // ============================================================
            // 16 ستون داده + 1 ستون دلیل خطا = 17 ستون
            // بدون هدر - دقیقاً مثل فایل ورودی
            // ============================================================
            int rowNum = 1;
            foreach (var (rowNumber, rowData, errorMessage) in rejectedRows)
            {
                // 16 ستون داده
                for (int col = 0; col < rowData.Count; col++)
                {
                    worksheet.Cell(rowNum, col + 1).Value = rowData[col];
                }

                // ستون 17: دلیل خطا
                worksheet.Cell(rowNum, 17).Value = errorMessage;
                rowNum++;
            }

            // ============================================================
            // تنظیم عرض ستون‌ها
            // ============================================================
            worksheet.Column(1).Width = 12;   // کد مرکز
            worksheet.Column(2).Width = 12;   // کد ساختمان
            worksheet.Column(3).Width = 25;   // نام ساختمان
            worksheet.Column(4).Width = 12;   // کد کلاس
            worksheet.Column(5).Width = 25;   // نام کلاس
            worksheet.Column(6).Width = 15;   // نوع کلاس
            worksheet.Column(7).Width = 10;   // وضعیت
            worksheet.Column(8).Width = 10;   // ظرفیت
            worksheet.Column(9).Width = 10;   // طبقه
            worksheet.Column(10).Width = 15;  // ظرفیت امتحانی
            worksheet.Column(11).Width = 15;  // وایت‌بورد
            worksheet.Column(12).Width = 15;  // پروژکتور
            worksheet.Column(13).Width = 30;  // امکانات
            worksheet.Column(14).Width = 30;  // توضیحات
            worksheet.Column(15).Width = 20;  // تلفن
            worksheet.Column(16).Width = 10;  // آموزشی
            worksheet.Column(17).Width = 50;  // دلیل خطا

            // ============================================================
            // تبدیل به Base64
            // ============================================================
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        // ============================================================
        // کلاس داخلی برای ردیف‌های اکسل
        // ============================================================
        private class SakhtemanKelassImportRow
        {
            public int RowNumber { get; set; }
            public string? CodeMarkaz { get; set; }
            public string? CodeSakhteman { get; set; }
            public string? NaamSakhteman { get; set; }
            public string? CodeClass { get; set; }
            public string? NaamClass { get; set; }
            public string? NoeClass { get; set; }
            public string? VazeeyatRaw { get; set; }
            public string? Zarfiat { get; set; }
            public string? Tabagheh { get; set; }
            public string? ZarfiatEmtahani { get; set; }
            public string? TedadWhiteboard { get; set; }
            public string? ProjectorRaw { get; set; }
            public string? Emkanat { get; set; }
            public string? Tozihat { get; set; }
            public string? ResponsiblePhone { get; set; }
            public string? AmoozeshiRaw { get; set; }
        }
    }
}