// Controllers/Edu/ReshtehController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Reshteh;
using PayamBack.Models.Edu;
using ClosedXML.Excel;


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
                        OnvanReshte = r.OnvanReshte,
                        TermVorood=r.TermVorood,
                        TermEamal=r.TermEamal,
                        Vazeeat=r.Vazeeat
                    })
                    .ToListAsync();

                _cache.Set(AllReshtehCacheKey, reshtehs, TimeSpan.FromHours(6));

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
                        OnvanReshte = r.OnvanReshte,
                        Vazeeat=r.Vazeeat
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
                        TermEamal = r.TermEamal,
                        Vazeeat=r.Vazeeat
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

                // 🔍 بررسی تکراری نبودن (CodeReshte + CodeMaghta + TermVorood + TermEamal)
                var exists = await _context.Reshtehs
                    .AnyAsync(r =>
                        r.CodeReshte == dto.CodeReshte &&
                        r.CodeMaghta == dto.CodeMaghta &&
                        r.TermVorood == dto.TermVorood &&
                        r.TermEamal == dto.TermEamal);

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
                    TermEamal = dto.TermEamal,
                    Vazeeat=dto.Vazeeat
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

                // 🔍 بررسی تکراری نبودن (CodeReshte + CodeMaghta + TermVorood + TermEamal)
                var targetCodeReshte = dto.CodeReshte ?? reshteh.CodeReshte;
                var targetCodeMaghta = dto.CodeMaghta ?? reshteh.CodeMaghta;
                var targetTermVorood = dto.TermVorood ?? reshteh.TermVorood;
                var targetTermEamal = dto.TermEamal ?? reshteh.TermEamal;
                var exists = await _context.Reshtehs
                    .AnyAsync(r =>
                        r.Id != id &&
                        r.CodeReshte == targetCodeReshte &&
                        r.CodeMaghta == targetCodeMaghta &&
                        r.TermVorood == targetTermVorood &&
                        r.TermEamal == targetTermEamal);
                if (exists)
                    return BadRequest(new
                    {
                        success = false,
                        message = "این رشته قبلاً با این کد، مقطع، ترم ورود و ترم اعمال ثبت شده است"
                    });

                reshteh.GrooheAmoozeshiId = dto.GrooheAmoozeshiId ?? reshteh.GrooheAmoozeshiId;
                reshteh.CodeMaghta = dto.CodeMaghta ?? reshteh.CodeMaghta;
                reshteh.Maghta = dto.Maghta ?? reshteh.Maghta;
                reshteh.CodeReshte = dto.CodeReshte ?? reshteh.CodeReshte;
                reshteh.OnvanReshte = dto.OnvanReshte ?? reshteh.OnvanReshte;
                reshteh.TermVorood = dto.TermVorood ?? reshteh.TermVorood;
                reshteh.TermEamal = dto.TermEamal ?? reshteh.TermEamal;
                reshteh.Vazeeat = dto.Vazeeat ?? reshteh.Vazeeat;

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
        [Authorize]
        public IActionResult ClearCache()
        {
            _cache.Remove(AllReshtehCacheKey);
            return Ok(new { success = true, message = "کش رشته‌ها پاک شد" });
        }

        // ============================================================
        // 8️⃣ آپلود گروهی رشته‌ها از فایل اکسل
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
                List<ReshtehImportRow> rows;
                try
                {
                    using var stream = file.OpenReadStream();
                    rows = ParseReshtehExcel(stream);
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
                // ۲. پیدا کردن همه گروه‌های آموزشی (یک بار برای همه)
                // ============================================================
                var allGroohes = await _context.GrooheAmoozeshis
                    .Select(g => new { g.Id, g.CodeDaneshkade, g.CodeGrooheAmoozeshi })
                    .ToListAsync();

                // ============================================================
                // ۳. پردازش هر ردیف
                // ============================================================
                var rejectedReshtehs = new List<RejectedReshtehImportDto>();
                var toInsert = new List<Reshteh>();
                int skippedCount = 0;

                foreach (var row in rows)
                {
                    // 🔍 چک فیلدهای اجباری
                    if (string.IsNullOrEmpty(row.CodeReshte) ||
                        string.IsNullOrEmpty(row.CodeMaghta))
                    {
                        rejectedReshtehs.Add(new RejectedReshtehImportDto
                        {
                            RowNumber = row.RowNumber,
                            OnvanReshte = row.OnvanReshte,
                            CodeReshte = row.CodeReshte,
                            CodeMaghta = row.CodeMaghta,
                            TermVorood = row.TermVorood,
                            TermEamal = row.TermEamal,
                            Reason = "کد رشته یا کد مقطع خالی است"
                        });
                        continue;
                    }

                    // 🔍 استخراج کد دانشکده و گروه از کد رشته
                    // کد رشته ۶ رقمی: DDGGRR
                    // DD = کد دانشکده، GG = کد گروه آموزشی، RR = کد رشته اصلی
                    string? codeDaneshkade = null;
                    string? codeGrooheAmoozeshi = null;

                    if (row.CodeReshte.Length == 6)
                    {
                        codeDaneshkade = row.CodeReshte.Substring(0, 2);
                        codeGrooheAmoozeshi = row.CodeReshte.Substring(2, 2);
                    }

                    // 🔍 پیدا کردن گروه آموزشی
                    int? grooheAmoozeshiId = null;
                    if (!string.IsNullOrEmpty(codeDaneshkade) && !string.IsNullOrEmpty(codeGrooheAmoozeshi))
                    {
                        var groohe = allGroohes.FirstOrDefault(g =>
                            g.CodeDaneshkade != null &&
                            g.CodeDaneshkade.StartsWith(codeDaneshkade) &&
                            g.CodeGrooheAmoozeshi != null &&
                            g.CodeGrooheAmoozeshi.StartsWith(codeGrooheAmoozeshi));

                        if (groohe != null)
                            grooheAmoozeshiId = groohe.Id;
                        // اگه پیدا نشد، null میمونه
                    }

                    // 🔍 چک تکراری بودن توی DB
                    var exists = await _context.Reshtehs
                        .AnyAsync(r =>
                            r.CodeReshte == row.CodeReshte &&
                            r.CodeMaghta == row.CodeMaghta &&
                            r.TermVorood == row.TermVorood &&
                            r.TermEamal == row.TermEamal);

                    if (exists)
                    {
                        skippedCount++;
                        continue;
                    }

                    // 🔍 چک تکراری بودن توی لیست toInsert (توی همین فایل)
                    var existsInFile = toInsert.Any(r =>
                        r.CodeReshte == row.CodeReshte &&
                        r.CodeMaghta == row.CodeMaghta &&
                        r.TermVorood == row.TermVorood &&
                        r.TermEamal == row.TermEamal);

                    if (existsInFile)
                    {
                        skippedCount++;
                        continue;
                    }

                    // ✅ اضافه کردن به لیست
                    toInsert.Add(new Reshteh
                    {
                        GrooheAmoozeshiId = grooheAmoozeshiId,
                        CodeMaghta = row.CodeMaghta,
                        Maghta = row.Maghta,
                        CodeReshte = row.CodeReshte,
                        OnvanReshte = row.OnvanReshte,
                        TermVorood = row.TermVorood,
                        TermEamal = row.TermEamal
                    });
                }

                // ============================================================
                // ۴. ذخیره در DB (تراکنشی)
                // ============================================================
                int insertedCount = 0;
                if (toInsert.Any())
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        await _context.Reshtehs.AddRangeAsync(toInsert);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        insertedCount = toInsert.Count;

                        // 🔥 پاک کردن کش
                        _cache.Remove(AllReshtehCacheKey);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        // اگه خطای کلی داد، همه رو به عنوان خطا برمی‌گردونیم
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "خطا در ذخیره رشته‌ها",
                            error = ex.Message
                        });
                    }
                }

                // ============================================================
                // ۵. ساخت فایل اکسل خطاها
                // ============================================================
                string? errorFileBase64 = null;
                if (rejectedReshtehs.Any())
                {
                    errorFileBase64 = GenerateReshtehErrorExcel(rejectedReshtehs);
                }

                return Ok(new
                {
                    success = true,
                    message = $"تعداد {insertedCount} رشته ثبت شد، {skippedCount} رشته تکراری Skip شد، {rejectedReshtehs.Count} رشته رد شد.",
                    data = new
                    {
                        inserted = insertedCount,
                        skipped = skippedCount,
                        rejected = rejectedReshtehs.Count,
                        total = rows.Count
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

        private string GenerateReshtehErrorExcel(List<RejectedReshtehImportDto> rejected)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ردشده‌ها");

            // هدر
            worksheet.Cell(1, 1).Value = "ردیف اکسل";
            worksheet.Cell(1, 2).Value = "عنوان رشته";
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
            foreach (var item in rejected)
            {
                worksheet.Cell(rowNum, 1).Value = item.RowNumber;
                worksheet.Cell(rowNum, 2).Value = item.OnvanReshte ?? "-";
                worksheet.Cell(rowNum, 3).Value = item.CodeReshte ?? "-";
                worksheet.Cell(rowNum, 4).Value = item.CodeMaghta ?? "-";
                worksheet.Cell(rowNum, 5).Value = item.TermVorood ?? "-";
                worksheet.Cell(rowNum, 6).Value = item.TermEamal ?? "-";
                worksheet.Cell(rowNum, 7).Value = item.Reason ?? "-";
                rowNum++;
            }

            // عرض ستون‌ها
            worksheet.Column(1).Width = 10;
            worksheet.Column(2).Width = 45;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 12;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 40;

            // تبدیل به Base64
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private List<ReshtehImportRow> ParseReshtehExcel(Stream stream)
        {
            var rows = new List<ReshtehImportRow>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var importRow = new ReshtehImportRow
                {
                    RowNumber = row,
                    CodeMaghta = GetCellString(worksheet, row, 2),
                    Maghta = GetCellString(worksheet, row, 3),
                    CodeReshte = GetCellString(worksheet, row, 6),
                    OnvanReshte = GetCellString(worksheet, row, 7),
                    TermVorood = GetCellString(worksheet, row, 8),
                    TermEamal = GetCellString(worksheet, row, 9)
                };

                // فقط ردیف‌هایی که کد رشته دارن
                if (!string.IsNullOrEmpty(importRow.CodeReshte))
                {
                    rows.Add(importRow);
                }
            }

            return rows;
        }

        private string? GetCellString(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            var value = cell.GetString()?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private class ReshtehImportRow
        {
            public int RowNumber { get; set; }
            public string? CodeMaghta { get; set; }      // ستون 2
            public string? Maghta { get; set; }          // ستون 3
            public string? CodeReshte { get; set; }      // ستون 6
            public string? OnvanReshte { get; set; }     // ستون 7
            public string? TermVorood { get; set; }      // ستون 8
            public string? TermEamal { get; set; }       // ستون 9
        }

        public class RejectedReshtehImportDto
        {
            public int RowNumber { get; set; }
            public string? OnvanReshte { get; set; }
            public string? CodeReshte { get; set; }
            public string? CodeMaghta { get; set; }
            public string? TermVorood { get; set; }
            public string? TermEamal { get; set; }
            public string? Reason { get; set; }
        }


    }
}