using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.DTOs.Core.Markaz;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // فقط کاربران لاگین شده
    public class MarkazController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MarkazController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه مراکز (فعال)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var markazes = await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .OrderBy(m => m.NaamMarkaz)
                    .Select(m => new MarkazListDto
                    {
                        Id = m.Id,
                        CodeMarkaz = m.CodeMarkaz ?? "",
                        NaamMarkaz = m.NaamMarkaz ?? "",
                        CodeOstan = m.CodeOstan ?? "",
                        NaamOstan = m.NaamOstan ?? "",
                        Vazeeyat = m.Vazeeyat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مراکز با موفقیت دریافت شد",
                    data = markazes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست مراکز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت اطلاعات یک مرکز
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var markaz = await _context.Markazes
                    .Where(m => m.Id == id)
                    .Select(m => new MarkazDetailDto
                    {
                        Id = m.Id,
                        CodeMarkaz = m.CodeMarkaz ?? "",
                        NaamMarkaz = m.NaamMarkaz ?? "",
                        CodeOstan = m.CodeOstan ?? "",
                        NaamOstan = m.NaamOstan ?? "",
                        VahedMarkaz = m.VahedMarkaz ?? "",
                        Nahiyeh = m.Nahiyeh ?? "",
                        MahalMarkaz = m.MahalMarkaz ?? "",
                        Adres = m.Adres ?? "",
                        CodePosti = m.CodePosti ?? "",
                        WebSite = m.WebSite ?? "",
                        Telefon = m.Telefon ?? "",
                        Vazeeyat = m.Vazeeyat ?? false
                    })
                    .FirstOrDefaultAsync();

                if (markaz == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مرکز مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات مرکز با موفقیت دریافت شد",
                    data = markaz
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات مرکز",
                    error = ex.Message
                });
            }
        }
    }
}