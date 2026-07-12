using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Term;
using PayamBack.Models.Edu;

namespace PayamBack.Controllers.Edu
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ادمین سامانه")]
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
        [AllowAnonymous]  // برای نمایش در صفحات عمومی
        public async Task<IActionResult> GetList()
        {
            try
            {
                var terms = await _context.Terms
                    .OrderByDescending(t => t.TermJari)
                    .Select(t => new TermListDto
                    {
                        CodeTerm = t.CodeTerm ?? "",
                        OnvanTerm = t.OnvanTerm ?? "",
                        TermJari = t.TermJari,
                        Vazeeyat = t.Vazeeyat ?? false
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
                        TermJari = t.TermJari,
                        TarikheDastrasi = t.TarikheDastrasi,
                        TarikheEraeeDars = t.TarikheEraeeDars,
                        TarikhePayanDars = t.TarikhePayanDars,
                        TarikheShorooClass = t.TarikheShorooClass,
                        TarikhePayanClass = t.TarikhePayanClass,
                        TarikheShorooMojavezMarakez = t.TarikheShorooMojavezMarakez,
                        TarikhePayanMojavezMarakez = t.TarikhePayanMojavezMarakez,
                        Vazeeyat = t.Vazeeyat ?? false
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

                var term = new Term
                {
                    CodeTerm = dto.CodeTerm,
                    OnvanTerm = dto.OnvanTerm,
                    TermJari = dto.TermJari,
                    TarikheDastrasi = dto.TarikheDastrasi,
                    TarikheEraeeDars = dto.TarikheEraeeDars,
                    TarikhePayanDars = dto.TarikhePayanDars,
                    TarikheShorooClass = dto.TarikheShorooClass,
                    TarikhePayanClass = dto.TarikhePayanClass,
                    TarikheShorooMojavezMarakez = dto.TarikheShorooMojavezMarakez,
                    TarikhePayanMojavezMarakez = dto.TarikhePayanMojavezMarakez,
                    Vazeeyat = dto.Vazeeyat ?? false
                };

                // ============================================================
                // اگر ترم فعال است، سایر ترم‌ها را غیرفعال کن
                // ============================================================
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
                term.TermJari = dto.TermJari ?? term.TermJari;
                term.TarikheDastrasi = dto.TarikheDastrasi ?? term.TarikheDastrasi;
                term.TarikheEraeeDars = dto.TarikheEraeeDars ?? term.TarikheEraeeDars;
                term.TarikhePayanDars = dto.TarikhePayanDars ?? term.TarikhePayanDars;
                term.TarikheShorooClass = dto.TarikheShorooClass ?? term.TarikheShorooClass;
                term.TarikhePayanClass = dto.TarikhePayanClass ?? term.TarikhePayanClass;
                term.TarikheShorooMojavezMarakez = dto.TarikheShorooMojavezMarakez ?? term.TarikheShorooMojavezMarakez;
                term.TarikhePayanMojavezMarakez = dto.TarikhePayanMojavezMarakez ?? term.TarikhePayanMojavezMarakez;

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
                        TermJari = t.TermJari,
                        TarikheShorooClass = t.TarikheShorooClass,
                        TarikhePayanClass = t.TarikhePayanClass
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
    }
}