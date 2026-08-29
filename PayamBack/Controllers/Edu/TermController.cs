using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Edu;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TermController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TermController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه ترم‌ها
        // ============================================================
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var terms = await _context.Terms
                    .OrderByDescending(t => t.TermJariShoroo)
                    .Select(t => new TermListDto
                    {
                        CodeTerm = t.CodeTerm ?? "",
                        OnvanTerm = t.OnvanTerm ?? "",
                        TermJariShoroo = t.TermJariShoroo,
                        TermJariPayan = t.TermJariPayan,
                        TarikheDastrasi = t.TarikheDastrasi,
                        TarikheEraeeDars = t.TarikheEraeeDars,
                        TarikhePayanDars = t.TarikhePayanDars,
                        TarikheShorooClass = t.TarikheShorooClass,
                        TarikhePayanClass = t.TarikhePayanClass,
                        TarikheShorooMojavezMarakez = t.TarikheShorooMojavezMarakez,
                        TarikhePayanMojavezMarakez = t.TarikhePayanMojavezMarakez,
                        Vazeeyat = t.Vazeeyat ?? false,
                        IsHaftegiRequired = t.IsHaftegiRequired
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست ترم‌ها با موفقیت دریافت شد",
                    data = terms
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست ترم‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت اطلاعات یک ترم
        // ============================================================
        [HttpGet("{codeTerm}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string codeTerm)
        {
            try
            {
                var term = await _context.Terms
                    .Where(t => t.CodeTerm == codeTerm)
                    .Select(t => new TermDetailDto
                    {
                        CodeTerm = t.CodeTerm ?? "",
                        OnvanTerm = t.OnvanTerm ?? "",
                        TermJariShoroo = t.TermJariShoroo,
                        TermJariPayan = t.TermJariPayan,
                        TarikheDastrasi = t.TarikheDastrasi,
                        TarikheEraeeDars = t.TarikheEraeeDars,
                        TarikhePayanDars = t.TarikhePayanDars,
                        TarikheShorooClass = t.TarikheShorooClass,
                        TarikhePayanClass = t.TarikhePayanClass,
                        TarikheShorooMojavezMarakez = t.TarikheShorooMojavezMarakez,
                        TarikhePayanMojavezMarakez = t.TarikhePayanMojavezMarakez,
                        Vazeeyat = t.Vazeeyat ?? false,
                        IsHaftegiRequired = t.IsHaftegiRequired
                    })
                    .FirstOrDefaultAsync();

                if (term == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ترم مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات ترم با موفقیت دریافت شد",
                    data = term
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات ترم",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد ترم جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TermCreateDto dto)
        {
            try
            {
                // بررسی تکراری نبودن CodeTerm
                var exists = await _context.Terms
                    .AnyAsync(t => t.CodeTerm == dto.CodeTerm);

                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "کد ترم تکراری است"
                    });
                }

                // تعیین مقدار پیش‌فرض IsHaftegiRequired
                bool isHaftegiRequired = dto.IsHaftegiRequired ?? true; //IsTermRequiresHaftegi(dto.CodeTerm, dto.TermJariShoroo);

                var term = new Term
                {
                    CodeTerm = dto.CodeTerm,
                    OnvanTerm = dto.OnvanTerm,
                    TermJariShoroo = dto.TermJariShoroo,
                    TermJariPayan = dto.TermJariPayan,
                    TarikheDastrasi = dto.TarikheDastrasi,
                    TarikheEraeeDars = dto.TarikheEraeeDars,
                    TarikhePayanDars = dto.TarikhePayanDars,
                    TarikheShorooClass = dto.TarikheShorooClass,
                    TarikhePayanClass = dto.TarikhePayanClass,
                    TarikheShorooMojavezMarakez = dto.TarikheShorooMojavezMarakez,
                    TarikhePayanMojavezMarakez = dto.TarikhePayanMojavezMarakez,
                    Vazeeyat = dto.Vazeeyat ?? false,
                    IsHaftegiRequired = isHaftegiRequired
                };

                // اگر ترم فعال است، سایر ترم‌ها را غیرفعال کن
                if (term.Vazeeyat == true)
                {
                    await DeactivateOtherTerms(term.CodeTerm);
                }

                await _context.Terms.AddAsync(term);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ترم با موفقیت ایجاد شد",
                    data = new { codeTerm = term.CodeTerm }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد ترم",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش ترم
        // ============================================================
        [HttpPut("update/{codeTerm}")]
        public async Task<IActionResult> Update(string codeTerm, [FromBody] TermUpdateDto dto)
        {
            try
            {
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.CodeTerm == codeTerm);

                if (term == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ترم مورد نظر یافت نشد"
                    });
                }

                // به‌روزرسانی فیلدها
                term.OnvanTerm = dto.OnvanTerm ?? term.OnvanTerm;
                term.TermJariShoroo = dto.TermJariShoroo ?? term.TermJariShoroo;
                term.TermJariPayan = dto.TermJariPayan ?? term.TermJariPayan;
                term.TarikheDastrasi = dto.TarikheDastrasi ?? term.TarikheDastrasi;
                term.TarikheEraeeDars = dto.TarikheEraeeDars ?? term.TarikheEraeeDars;
                term.TarikhePayanDars = dto.TarikhePayanDars ?? term.TarikhePayanDars;
                term.TarikheShorooClass = dto.TarikheShorooClass ?? term.TarikheShorooClass;
                term.TarikhePayanClass = dto.TarikhePayanClass ?? term.TarikhePayanClass;
                term.TarikheShorooMojavezMarakez = dto.TarikheShorooMojavezMarakez ?? term.TarikheShorooMojavezMarakez;
                term.TarikhePayanMojavezMarakez = dto.TarikhePayanMojavezMarakez ?? term.TarikhePayanMojavezMarakez;

                // به‌روزرسانی IsHaftegiRequired
                if (dto.IsHaftegiRequired.HasValue)
                {
                    term.IsHaftegiRequired = dto.IsHaftegiRequired.Value;
                }

                // اگر وضعیت فعال تغییر کرده است
                if (dto.Vazeeyat.HasValue && dto.Vazeeyat.Value != term.Vazeeyat)
                {
                    term.Vazeeyat = dto.Vazeeyat.Value;

                    // اگر ترم فعال شده، سایر ترم‌ها را غیرفعال کن
                    if (term.Vazeeyat == true)
                    {
                        await DeactivateOtherTerms(term.CodeTerm);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ترم با موفقیت بروزرسانی شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بروزرسانی ترم",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف ترم
        // ============================================================
        [HttpDelete("delete/{codeTerm}")]
        public async Task<IActionResult> Delete(string codeTerm)
        {
            try
            {
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.CodeTerm == codeTerm);

                if (term == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ترم مورد نظر یافت نشد"
                    });
                }

                // جلوگیری از حذف ترم جاری (فعال)
                if (term.Vazeeyat == true)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "حذف ترم جاری امکان‌پذیر نمی‌باشد. ابتدا ترم جاری را غیرفعال کنید."
                    });
                }

                _context.Terms.Remove(term);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ترم با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف ترم",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ دریافت ترم فعال
        // ============================================================
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveTerm()
        {
            try
            {
                var activeTerm = await _context.Terms
                    .Where(t => t.Vazeeyat == true)
                    .Select(t => new TermActiveDto
                    {
                        CodeTerm = t.CodeTerm ?? "",
                        OnvanTerm = t.OnvanTerm ?? "",
                        TermJariShoroo = t.TermJariShoroo,
                        TermJariPayan = t.TermJariPayan,
                        TarikheDastrasi = t.TarikheDastrasi,
                        TarikheEraeeDars = t.TarikheEraeeDars,
                        TarikhePayanDars = t.TarikhePayanDars,
                        TarikheShorooClass = t.TarikheShorooClass,
                        TarikhePayanClass = t.TarikhePayanClass,
                        TarikheShorooMojavezMarakez = t.TarikheShorooMojavezMarakez,
                        TarikhePayanMojavezMarakez = t.TarikhePayanMojavezMarakez,
                        Vazeeyat = t.Vazeeyat,
                        IsHaftegiRequired = t.IsHaftegiRequired
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = activeTerm == null ? "ترم فعالی وجود ندارد" : "ترم فعال با موفقیت دریافت شد",
                    data = activeTerm
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت ترم فعال",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // متد کمکی: غیرفعال کردن سایر ترم‌ها
        // ============================================================
        private async Task DeactivateOtherTerms(string currentCodeTerm)
        {
            var otherTerms = await _context.Terms
                .Where(t => t.CodeTerm != currentCodeTerm && t.Vazeeyat == true)
                .ToListAsync();

            foreach (var term in otherTerms)
            {
                term.Vazeeyat = false;
            }
        }

        // ============================================================
        // متد کمکی: تعیین مقدار پیش‌فرض IsHaftegiRequired
        // ============================================================
        /*private bool IsTermRequiresHaftegi(string codeTerm, DateOnly? termJariShoroo)
        {
            // روش 1: بررسی کد ترم
            // فرض می‌کنیم ترم‌های تابستان با کد خاصی مشخص می‌شوند
            // مثلاً اگر کد ترم شامل "2" باشد (نیمسال دوم تابستان)
            if (!string.IsNullOrEmpty(codeTerm))
            {
                // این منطق را بر اساس فرمت کد ترم خودت تنظیم کن
                // مثلاً اگر کد ترم به "2" ختم می‌شود (تابستان)
                if (codeTerm.EndsWith("2") || codeTerm.Contains("-2"))
                    return false;
            }

            // روش 2: بررسی تاریخ شروع ترم
            if (termJariShoroo.HasValue)
            {
                var month = termJariShoroo.Value.Month;
                // تیر (7)، مرداد (8)، شهریور (9) ← تابستان
                if (month >= 7 && month <= 9)
                    return false;
            }

            return true;
        }*/
    }
}